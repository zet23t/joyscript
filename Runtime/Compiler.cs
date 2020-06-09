using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace JoyScript
{
    public class Compiler
    {
        private class Reader
        {
            public int CurrentLine { get; private set; }
            private string code;
            private int pos;

            private int maxCalls = 1000;

            public bool IsDone => pos >= code.Length || maxCalls-- < 0;

            public int CurrentPosition => pos;

            public string GetSubString(int start, int stop)
            {
                return code.Substring(start, stop - start);
            }

            public void Load(string code)
            {
                this.code = code;
                pos = 0;
                CurrentLine = 1;
            }

            public char GetNextNonWS()
            {
                while (pos < code.Length && Peek() <= ' ')
                {
                    Next();
                }
                return Peek();
            }

            public char Next()
            {
                if (Peek() == '\n')
                {
                    CurrentLine += 1;
                }
                pos += 1;
                return Peek();
            }

            public string GetNextToken(string validStartLetters, string validLetters)
            {
                if (validStartLetters.IndexOf(GetNextNonWS()) < 0)
                {
                    return null;
                }
                if (IsDone)
                {
                    return null;
                }

                int start = pos;

                while (!IsDone && validLetters.IndexOf(Peek()) >= 0)
                {
                    Next();
                }

                if (start == pos)
                {
                    return null;
                }

                return code.Substring(start, pos - start);
            }

            public char Peek() => pos < code.Length ? code[pos] : (char) 0;
        }

        private Reader reader = new Reader();
        private List<Value> program;
        private const string ValidIdentifierStartLetters = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string ValidIdentifierLetters = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static HashSet<string> keywords = new HashSet<string>()
        {
            "if",
            "then",
            "else",
            "elseif",
            "end",
            "function",
            "goto",
            "for",
            "while",
            "do",
            "until",
        };

        public List<Value> Compile(string text)
        {
            program = new List<Value>();
            reader.Load(text);

            CompileBlock(new HashSet<string>());
            return program;
        }

        private string CompileBlock(HashSet<string> terminators)
        {
            Log("Compile block until => "+string.Join("|", terminators));
            while (!reader.IsDone)
            {
                string identifier = reader.GetNextToken(ValidIdentifierStartLetters, ValidIdentifierLetters);
                if (identifier == null && terminators.Count == 0)
                {
                    Log("Terminated");
                    return null;
                }
                else if (identifier == null)
                {
                    throw SyntaxError("Unexpected block termination, expected: " + string.Join(" | ", terminators));
                }

                Log(" ? "+identifier);

                char v = reader.GetNextNonWS();
                if (v == '=')
                {
                    reader.Next();
                    if (TryReadExpression())
                    {

                    }
                    continue;
                }
                if (v == '(')
                {
                    // function call
                    reader.Next();
                    if (TryReadExpression(')'))
                    {

                    }
                    Append(OpCode.LoadGlobalKeyLiteral, identifier);
                    Append(OpCode.PushValueLiteral, 1);
                    Append(OpCode.Call);
                    continue;
                }
                throw new SyntaxError("unexpected character: "+v);
            }
            throw new NotImplementedException();
        }

        private void Log(string identifier)
        {
            Debug.Log(identifier);
        }

        private void Append(params Value[] values)
        {
            foreach(Value v in values)
            {
                program.Add(v);
            }
        }

        private bool TryReadExpression(char closing = (char) 0)
        {
            bool isValid = false;
            while (!reader.IsDone)
            {
                Log("=> "+reader.GetNextNonWS());
                if (closing == reader.GetNextNonWS())
                {
                    return isValid;
                }
                if (reader.GetNextNonWS() == '"')
                {
                    Append(OpCode.PushValueLiteral, ReadStringLiteral());
                    continue;
                }
                if (reader.GetNextNonWS() == ')')
                {
                    throw SyntaxError("Unexpected )");
                }
                if (reader.GetNextNonWS() == '(')
                {
                    reader.Next();
                    if (!TryReadExpression(')'))
                    {
                        return false;
                    }
                }
                string token = reader.GetNextToken(ValidIdentifierStartLetters, ValidIdentifierLetters);
                if (token != null)
                {
                    if (keywords.Contains(token))
                    {
                        throw SyntaxError("Unexpected keyword");
                    }
                    continue;
                }
                throw SyntaxError("Expected identifier");
            }
            throw SyntaxError("Unexpected end of expression");
        }

        private string ReadStringLiteral()
        {
            StringBuilder sb = new StringBuilder();
            while (!reader.IsDone && ((reader.Next()) != '"'))
            {
                char chr = reader.Peek();
                if (chr == '\\')
                {
                    switch (reader.Next())
                    {
                        case '"': 
                            sb.Append('"');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        default:
                            sb.Append('\\');
                            break;
                    }
                    continue;
                }
                sb.Append(chr);
            }
            if (reader.Peek() != '"')
            {
                throw SyntaxError("Unterminated string sequence");
            }
            reader.Next();
            return sb.ToString();
        }

        private SyntaxError SyntaxError(string message)
        {
            return new SyntaxError("Line " + reader.CurrentLine + " --- " + message);
        }
    }
}