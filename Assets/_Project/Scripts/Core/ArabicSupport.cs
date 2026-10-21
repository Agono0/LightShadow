using System.Collections.Generic;
using System.Text;

namespace LightShadow.Core
{
    // Unity's legacy UI text has no Arabic shaping and no bidi algorithm. This
    // small renderer-independent helper converts logical Arabic into the
    // presentation forms and visual order a left-to-right Text/TextMesh expects.
    // It is deliberately conservative: Arabic words are shaped, Latin/digit runs
    // keep their internal order, and the run order is reversed for RTL.
    public static class ArabicSupport
    {
        struct Forms
        {
            public char isolated;
            public char final;
            public char initial;
            public char medial;

            public Forms(char isolated, char final, char initial = '\0', char medial = '\0')
            {
                this.isolated = isolated;
                this.final = final;
                this.initial = initial;
                this.medial = medial;
            }

            public bool CanJoinNext => initial != '\0';
        }

        static readonly Dictionary<char, Forms> LetterForms = new Dictionary<char, Forms>
        {
            { '\u0621', new Forms('\uFE80', '\uFE80') },                    // hamza
            { '\u0622', new Forms('\uFE81', '\uFE82') },                    // alef madda
            { '\u0623', new Forms('\uFE83', '\uFE84') },                    // alef hamza above
            { '\u0624', new Forms('\uFE85', '\uFE86') },                    // waw hamza
            { '\u0625', new Forms('\uFE87', '\uFE88') },                    // alef hamza below
            { '\u0626', new Forms('\uFE89', '\uFE8A', '\uFE8B', '\uFE8C') },// yeh hamza
            { '\u0627', new Forms('\uFE8D', '\uFE8E') },                    // alef
            { '\u0628', new Forms('\uFE8F', '\uFE90', '\uFE91', '\uFE92') },// beh
            { '\u0629', new Forms('\uFE93', '\uFE94') },                    // teh marbuta
            { '\u062A', new Forms('\uFE95', '\uFE96', '\uFE97', '\uFE98') },// teh
            { '\u062B', new Forms('\uFE99', '\uFE9A', '\uFE9B', '\uFE9C') },// theh
            { '\u062C', new Forms('\uFE9D', '\uFE9E', '\uFE9F', '\uFEA0') },// jeem
            { '\u062D', new Forms('\uFEA1', '\uFEA2', '\uFEA3', '\uFEA4') },// hah
            { '\u062E', new Forms('\uFEA5', '\uFEA6', '\uFEA7', '\uFEA8') },// khah
            { '\u062F', new Forms('\uFEA9', '\uFEAA') },                    // dal
            { '\u0630', new Forms('\uFEAB', '\uFEAC') },                    // thal
            { '\u0631', new Forms('\uFEAD', '\uFEAE') },                    // reh
            { '\u0632', new Forms('\uFEAF', '\uFEB0') },                    // zain
            { '\u0633', new Forms('\uFEB1', '\uFEB2', '\uFEB3', '\uFEB4') },// seen
            { '\u0634', new Forms('\uFEB5', '\uFEB6', '\uFEB7', '\uFEB8') },// sheen
            { '\u0635', new Forms('\uFEB9', '\uFEBA', '\uFEBB', '\uFEBC') },// sad
            { '\u0636', new Forms('\uFEBD', '\uFEBE', '\uFEBF', '\uFEC0') },// dad
            { '\u0637', new Forms('\uFEC1', '\uFEC2', '\uFEC3', '\uFEC4') },// tah
            { '\u0638', new Forms('\uFEC5', '\uFEC6', '\uFEC7', '\uFEC8') },// zah
            { '\u0639', new Forms('\uFEC9', '\uFECA', '\uFECB', '\uFECC') },// ain
            { '\u063A', new Forms('\uFECD', '\uFECE', '\uFECF', '\uFED0') },// ghain
            { '\u0641', new Forms('\uFED1', '\uFED2', '\uFED3', '\uFED4') },// feh
            { '\u0642', new Forms('\uFED5', '\uFED6', '\uFED7', '\uFED8') },// qaf
            { '\u0643', new Forms('\uFED9', '\uFEDA', '\uFEDB', '\uFEDC') },// kaf
            { '\u0644', new Forms('\uFEDD', '\uFEDE', '\uFEDF', '\uFEE0') },// lam
            { '\u0645', new Forms('\uFEE1', '\uFEE2', '\uFEE3', '\uFEE4') },// meem
            { '\u0646', new Forms('\uFEE5', '\uFEE6', '\uFEE7', '\uFEE8') },// noon
            { '\u0647', new Forms('\uFEE9', '\uFEEA', '\uFEEB', '\uFEEC') },// heh
            { '\u0648', new Forms('\uFEED', '\uFEEE') },                    // waw
            { '\u0649', new Forms('\uFEEF', '\uFEF0') },                    // alef maksura
            { '\u064A', new Forms('\uFEF1', '\uFEF2', '\uFEF3', '\uFEF4') } // yeh
        };

        static readonly Dictionary<char, char[]> LamAlef = new Dictionary<char, char[]>
        {
            { '\u0622', new[] { '\uFEF5', '\uFEF6' } },
            { '\u0623', new[] { '\uFEF7', '\uFEF8' } },
            { '\u0625', new[] { '\uFEF9', '\uFEFA' } },
            { '\u0627', new[] { '\uFEFB', '\uFEFC' } }
        };

        public static bool ContainsArabic(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] >= '\u0600' && text[i] <= '\u06FF') return true;
            }
            return false;
        }

        // Converts one logical string (possibly multi-line) to display order.
        public static string Fix(string text)
        {
            if (string.IsNullOrEmpty(text) || !ContainsArabic(text)) return text;

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++) lines[i] = FixLine(lines[i]);
            return string.Join("\n", lines);
        }

        static string FixLine(string line)
        {
            if (string.IsNullOrEmpty(line)) return line;

            var tokens = Tokenize(line);
            var builder = new StringBuilder(line.Length + 16);

            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                Token token = tokens[i];
                builder.Append(token.Arabic ? ShapeWord(token.Text) : token.Text);
            }

            return Mirror(builder.ToString());
        }

        struct Token
        {
            public string Text;
            public bool Arabic;
        }

        static List<Token> Tokenize(string line)
        {
            var tokens = new List<Token>();
            var run = new StringBuilder();
            bool? runArabic = null;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (char.IsWhiteSpace(c))
                {
                    Flush(tokens, run, runArabic);
                    runArabic = null;
                    tokens.Add(new Token { Text = c.ToString(), Arabic = false });
                    continue;
                }

                bool arabic = !IsLatinOrDigit(c);
                if (runArabic.HasValue && runArabic.Value != arabic)
                {
                    Flush(tokens, run, runArabic);
                }

                runArabic = arabic;
                run.Append(c);
            }

            Flush(tokens, run, runArabic);
            return tokens;
        }

        static void Flush(List<Token> tokens, StringBuilder run, bool? arabic)
        {
            if (run.Length == 0) return;
            tokens.Add(new Token { Text = run.ToString(), Arabic = arabic.HasValue && arabic.Value });
            run.Clear();
        }

        // Latin letters, digits and the punctuation that belongs to a number or
        // an inline English fragment keep their left-to-right order.
        static bool IsLatinOrDigit(char c)
        {
            if (char.IsDigit(c)) return true;
            if (c >= 'A' && c <= 'Z') return true;
            if (c >= 'a' && c <= 'z') return true;
            switch (c)
            {
                case '+':
                case '-':
                case '/':
                case '%':
                case '=':
                case '.':
                case ',':
                case ':':
                case ';':
                case '!':
                case '?':
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                    return true;
                default:
                    return false;
            }
        }

        static string ShapeWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            var shaped = new StringBuilder(word.Length + 8);

            for (int i = 0; i < word.Length; i++)
            {
                char c = word[i];
                if (!LetterForms.TryGetValue(c, out Forms forms))
                {
                    shaped.Append(c);
                    continue;
                }

                char previous = PreviousBaseLetter(word, i);
                int nextIndex = NextBaseLetterIndex(word, i);
                char next = nextIndex >= 0 ? word[nextIndex] : '\0';

                bool previousJoins = previous != '\0' && LetterForms.TryGetValue(previous, out Forms previousForms)
                    && previousForms.CanJoinNext;

                // Lam + alef is always one ligature glyph, never two letters.
                if (c == '\u0644' && next != '\0' && LamAlef.TryGetValue(next, out char[] ligature))
                {
                    shaped.Append(previousJoins ? ligature[1] : ligature[0]);
                    i = nextIndex; // skip the alef
                    continue;
                }

                bool nextJoins = next != '\0' && forms.CanJoinNext
                    && LetterForms.ContainsKey(next);

                char output;
                if (previousJoins && nextJoins && forms.medial != '\0') output = forms.medial;
                else if (previousJoins && forms.final != '\0') output = forms.final;
                else if (nextJoins && forms.initial != '\0') output = forms.initial;
                else output = forms.isolated;

                shaped.Append(output);
            }

            // Presentation forms are laid out right-to-left.
            char[] characters = shaped.ToString().ToCharArray();
            System.Array.Reverse(characters);
            return new string(characters);
        }

        static char PreviousBaseLetter(string word, int index)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                if (IsDiacritic(word[i])) continue;
                return word[i];
            }
            return '\0';
        }

        static int NextBaseLetterIndex(string word, int index)
        {
            for (int i = index + 1; i < word.Length; i++)
            {
                if (IsDiacritic(word[i])) continue;
                return LetterForms.ContainsKey(word[i]) ? i : -1;
            }
            return -1;
        }

        static bool IsDiacritic(char c)
        {
            return (c >= '\u064B' && c <= '\u065F') || c == '\u0670' || c == '\u06D6' || c == '\u06D7'
                || c == '\u06D8' || c == '\u06D9' || c == '\u06DA' || c == '\u06DB' || c == '\u06DC'
                || c == '\u06DF' || c == '\u06E0' || c == '\u06E1' || c == '\u06E2' || c == '\u06E3'
                || c == '\u06E4' || c == '\u06E7' || c == '\u06E8' || c == '\u06EA' || c == '\u06EB'
                || c == '\u06EC' || c == '\u06ED';
        }

        static string Mirror(string text)
        {
            char[] characters = text.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                switch (characters[i])
                {
                    case '(': characters[i] = ')'; break;
                    case ')': characters[i] = '('; break;
                    case '[': characters[i] = ']'; break;
                    case ']': characters[i] = '['; break;
                    case '{': characters[i] = '}'; break;
                    case '}': characters[i] = '{'; break;
                    case '<': characters[i] = '>'; break;
                    case '>': characters[i] = '<'; break;
                }
            }
            return new string(characters);
        }
    }
}
