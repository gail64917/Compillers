using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Меняем на ввод из файла
            //Grammar.Rule r1 = new Grammar.Rule("A", "B");
            //Grammar.Rule r2 = new Grammar.Rule("B", "C");
            //Grammar.Rule r3 = new Grammar.Rule("B", "f");
            //Grammar grammar = new Grammar(new List<string>() { "A", "B", "C" }, new List<string>() { "f" }, new List<Grammar.Rule>() { r1, r2, r3 }, new List<string>());



            //String line;
            try
            {
                StreamReader sr = new StreamReader("..\\Input.txt");

                int numOfNonTerms = Int32.Parse(sr.ReadLine());

                List<string> N = new List<string>();
                List<string> T = new List<string>();
                List<string> Startingstring = new List<string>();
                List<Grammar.Rule> R = new List<Grammar.Rule>();

                for (int i=0; i< numOfNonTerms; i++)
                {
                    N.Add(sr.ReadLine());
                }

                int numOfTerms = Int32.Parse(sr.ReadLine());

                for (int i = 0; i < numOfTerms; i++)
                {
                    T.Add(sr.ReadLine());
                }

                int numOfRules = Int32.Parse(sr.ReadLine());

                for (int i = 0; i < numOfRules; i++)
                {
                    List<Grammar.Rule> rulesDivided = Grammar.Rule.Parse(sr.ReadLine());
                    foreach(Grammar.Rule item in rulesDivided)
                    {
                        R.Add(item);
                    }
                }

                Startingstring.Add(sr.ReadLine());

                Grammar grammar = new Grammar(N, T, R, Startingstring);
                Grammar gr = grammar.DeleteChainRules();

                Console.WriteLine("Input: " + grammar.Info());
                Console.WriteLine("Output: " + gr.Info());
                Console.ReadLine();

                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            //


            
        }
    }


    public class Grammar
    {

        public class Rule
        {
            public string leftSide;
            public string rightSide;

            public Rule(string l, string r)
            {
                leftSide = l;
                rightSide = r;
            }

            public static List<Rule> Parse(string input)
            {
                List<Rule> DividedRules = new List<Rule>();
                input = input.Trim(' ');
                string[] sides = input.Split(new string[] { "->" }, StringSplitOptions.None);
                string[] rightSides = sides[1].Split("|");
                foreach (string rightside in rightSides)
                {
                    Rule newRule = new Rule(sides[0], rightside);
                    DividedRules.Add(newRule);
                }
                return DividedRules;
            }

            public string Info()
            {
                return string.Concat(leftSide, "->", rightSide);
            }
        }

        private List<string> N, T, Startingstring;
        private List<Rule> R;

        /// <summary>
        /// Конструктор для ввода из файла
        /// </summary>
        /// <param name="N">Нетерминалы</param>
        /// <param name="T">Терминалы</param>
        /// <param name="R">Правила</param>
        /// <param name="Startingstring">Начальный символ</param>
        public Grammar(List<string> _N, List<string> _T, List<string> _R, List<string> _Startingstring)
        {
            N = _N;
            T = _T;
            Startingstring = _Startingstring;
            foreach(string rule in _R)
            {
                List<Rule> divided = Rule.Parse(rule);
                foreach(Rule item in divided)
                {
                    R.Add(item);
                } 
            }
        }

        /// <summary>
        /// Конструктор для дебага
        /// </summary>
        public Grammar(List<string> _N, List<string> _T, List<Rule> _R, List<string> _Startingstring)
        {
            N = _N;
            T = _T;
            Startingstring = _Startingstring;
            R = _R;
        }

        public string Info()
        {
            return "\r\n(\r\n {" + String.Join(",", N.ToArray()) + "},\r\n {" + String.Join(",", T.ToArray()) + "},\r\n {" + PrintRules() + "},\r\n {" + String.Join(" ", Startingstring.ToArray()) + "}\r\n)\r\n";
        }

        //если справа в правиле есть нетерминал - то смотрим, можно ли его заменить на терминал или нетерминал (в этом случае тоже перебираем). 
        //Если нетерминал можно заменить на несколько терминалов/нетерминалов - каждую ветвь возвращаем 

        private string PrintRules()
        {
            string output = "";
            foreach(Rule rule in R)
            {
                output += rule.leftSide + "->" + rule.rightSide + "; ";
            }
            return output;
        }

        //Проходим по всем правилам и пытаемся сократить каждое имеющееся
        public Grammar DeleteChainRules()
        {
            List<Rule> r = new List<Rule>();
            foreach(Rule rule in R)
            {
                List<Rule> rMini = DeleteChain(rule);
                foreach(Rule mini in rMini)
                {
                    r.Add(mini);
                }
            }
            Grammar gr = new Grammar(N, T, r, Startingstring);
            return gr;
        }

        //подаем правило на вход в функцию. На выходе набор Правил, которые получаются путем удаления цепных нетерминалов
        private List<Rule> DeleteChain(Rule input)
        {
            List<Rule> Rules = new List<Rule>();
            bool nothingChanged = true;
            
            //костыль начат
            if(input.rightSide.Length > 1)
            {
                Rules.Add(input);
                return Rules;
            }
            //костыль окончен

            foreach (char symbolInRight in input.rightSide)
            {
                //порождающие правила ( A -> A + B) - оставляем и скипаем правила
                if (symbolInRight.ToString().Equals(input.leftSide))
                {
                    break;
                }
                //терминалы не трогаем
                if (T.Any(s => s.Equals(symbolInRight.ToString())))
                {
                    //Rules.Add(input);
                    continue;
                }
                //если справа видим нетерминал - пытаемся его "сократить"
                if (N.Any(s => s.Equals(symbolInRight.ToString())))
                {
                    //ищем правила, начинающиеся с данного нетерминала
                    foreach(Rule rule in R)
                    {
                        if(rule.leftSide.Equals(symbolInRight.ToString()))
                        {
                            nothingChanged = false;
                            Rule TempRule = new Rule(input.leftSide, rule.rightSide);
                            List<Rule> child = DeleteChain(TempRule);
                            foreach (Rule childRule in child)
                            {
                                Rules.Add(childRule);
                            }
                        }
                    }
                }
            }
            if (nothingChanged)
            {
                Rules.Add(input);
            }
            return Rules;
        }
    }
}