using System;
using System.Collections.Generic;

namespace LightShadow.Contrib.Usef.LevelLogic
{
    public sealed class LevelGraphReport
    {
        // Problems in the model: duplicate ids, requirements nothing can ever grant,
        // steps that can never run, a goal nothing grants.
        public readonly List<string> Errors = new List<string>();
        // Suspicious but not wrong: a step that grants nothing, a token no step uses.
        public readonly List<string> Warnings = new List<string>();
        // Step ids in the order a simulated run unlocked them (one valid witness order).
        public readonly List<string> WitnessOrder = new List<string>();
        public readonly HashSet<string> ReachableSteps = new HashSet<string>();
        // True only when the goal token is reachable in the MODELED graph.
        // This is not a statement about the running game.
        public bool GoalReachable;
        public bool Ok { get { return Errors.Count == 0 && GoalReachable; } }
    }

    public static class LevelGraphValidator
    {
        static bool Enabled(LevelStep step, HashSet<string> held)
        {
            foreach (string need in step.RequiresAll) if (!held.Contains(need)) return false;
            if (step.RequiresAny.Length == 0) return true;
            foreach (string any in step.RequiresAny) if (held.Contains(any)) return true;
            return false;
        }

        // Forward simulation: keep running every step whose requirements are held
        // until nothing new unlocks. Returns the step ids in the order they ran.
        public static List<string> Simulate(LevelGraph graph, HashSet<string> held, string skipStepId)
        {
            var order = new List<string>();
            var done = new HashSet<string>();
            bool progressed = true;
            while (progressed)
            {
                progressed = false;
                foreach (LevelStep step in graph.Steps)
                {
                    if (step.Id == skipStepId || done.Contains(step.Id)) continue;
                    if (!Enabled(step, held)) continue;
                    done.Add(step.Id);
                    order.Add(step.Id);
                    foreach (string grant in step.Grants) held.Add(grant);
                    progressed = true;
                }
            }
            return order;
        }

        public static LevelGraphReport Validate(LevelGraph graph)
        {
            var report = new LevelGraphReport();
            if (graph == null)
            {
                report.Errors.Add("graph is null");
                return report;
            }

            var ids = new HashSet<string>();
            var granted = new HashSet<string>(graph.InitialTokens);
            var required = new HashSet<string>();
            foreach (LevelStep step in graph.Steps)
            {
                if (string.IsNullOrEmpty(step.Id)) report.Errors.Add("a step has an empty id");
                else if (!ids.Add(step.Id)) report.Errors.Add("duplicate step id: " + step.Id);
                foreach (string g in step.Grants) granted.Add(g);
                foreach (string r in step.RequiresAll) required.Add(r);
                foreach (string r in step.RequiresAny) required.Add(r);
                if (step.Grants.Length == 0) report.Warnings.Add("step grants nothing: " + step.Id);
            }

            if (string.IsNullOrEmpty(graph.GoalToken)) report.Errors.Add("no goal token set");
            else if (!granted.Contains(graph.GoalToken)) report.Errors.Add("nothing grants the goal token: " + graph.GoalToken);

            foreach (string r in required)
            {
                if (!granted.Contains(r)) report.Errors.Add("required token is never granted and not held at start: " + r);
            }
            foreach (string g in granted)
            {
                if (!required.Contains(g) && g != graph.GoalToken && !graph.InitialTokens.Contains(g))
                    report.Warnings.Add("token is granted but no step requires it: " + g);
            }

            var held = new HashSet<string>(graph.InitialTokens);
            report.WitnessOrder.AddRange(Simulate(graph, held, null));
            foreach (string id in report.WitnessOrder) report.ReachableSteps.Add(id);
            report.GoalReachable = !string.IsNullOrEmpty(graph.GoalToken) && held.Contains(graph.GoalToken);

            foreach (LevelStep step in graph.Steps)
            {
                if (!report.ReachableSteps.Contains(step.Id))
                    report.Errors.Add("step can never run (blocked, or part of a requirement cycle): " + step.Id);
            }
            return report;
        }

        // Steps whose removal makes the goal unreachable: single points of failure.
        // If the real game makes one of these optional or breakable, the level can soft-lock.
        public static List<string> CriticalSteps(LevelGraph graph)
        {
            var critical = new List<string>();
            foreach (LevelStep step in graph.Steps)
            {
                var held = new HashSet<string>(graph.InitialTokens);
                Simulate(graph, held, step.Id);
                if (!held.Contains(graph.GoalToken)) critical.Add(step.Id);
            }
            return critical;
        }
    }
}
