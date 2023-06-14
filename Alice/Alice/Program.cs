using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Alice
{
    class Program
    {
        
        static void Main(string[] args)
        {
            int questions = int.Parse(Console.ReadLine());
            string alph = Console.ReadLine();
            char[] abc = alph.ToCharArray();
            PopulationController.Alice(abc, questions);
        }
    }

    public enum TypeNode { Root, Union, Concat, Power, Char }
    public enum WordType { SAW, AW, DW }
    public struct EvaluationType
    {
        public float SubsequenceValue;
        public float AcceptedValue;
        public float DeniedValue;

        public EvaluationType(float subVal, float accVal, float denVal)
        {
            SubsequenceValue = subVal;
            AcceptedValue = accVal;
            DeniedValue = denVal;
        }
    }

    public class Subject
    {
        //Instance Properties
        public bool IsLeftSon { get; set; }
        public Subject Parent { get; set; }
        public Subject[] Siblings { get; set; }
        public TypeNode Type { get; set; }
        public char Symbol { get; set; }
        int AmountP { get; set; }
        int HeightP { get; set; }
        int AstericsP { get; set; }

        //Static Properties
        public static int Amount(Subject node)
        { return node == null ? 0 : node.AmountP; }
        public static int Height(Subject node)
        { return node == null ? 0 : node.HeightP; }
        public static int Asterics(Subject node)
        { return node == null ? 0 : node.AstericsP; }

        //Static Methods
        public static Subject CreateRandomSubject(int k, char[] abc, Random rnd)
        {
            return CreateRandomSubject(k, abc, true, rnd);
        }
        static Subject CreateRandomSubject(int k, char[] abc, bool root, Random rnd)
        {
            Subject node = null;
            if (root)
            {
                node = new Subject(TypeNode.Root);
                node.AppendChild(CreateRandomSubject(k, abc, false, rnd), 0, true);
            }
            else
            {
                if (k <= 1)
                {
                    int i = rnd.Next(abc.Length);
                    node = new Subject(abc[i]);
                }
                else
                {
                    int x = rnd.Next(3);
                    int m = (k - 1) / 2;
                    int n = k / 2;
                    switch (x)
                    {
                        case 0:
                            node = new Subject(TypeNode.Union);
                            node.AppendChild(CreateRandomSubject(m, abc, false, rnd), 0, true);
                            node.AppendChild(CreateRandomSubject(n, abc, false, rnd), 1, true);
                            break;
                        case 1:
                            node = new Subject(TypeNode.Concat);
                            node.AppendChild(CreateRandomSubject(m, abc, false, rnd), 0, true);
                            node.AppendChild(CreateRandomSubject(n, abc, false, rnd), 1, true);
                            break;
                        case 2:
                            node = new Subject(TypeNode.Power);
                            node.AppendChild(CreateRandomSubject(k - 1, abc, false, rnd), 0, true);
                            break;
                        default:
                            break;
                    }
                }
            }
            return node;
        }
        public static Subject Select(Subject node, int i)
        {
            if (i >= node.AmountP) throw new Exception("Ploff");
            if (node.Type == TypeNode.Root) return Select(node.Siblings[0], i);

            int x = Amount(node.Siblings[0]);

            if (x == i) return node;
            if (x > i) return Select(node.Siblings[0], i);
            return Select(node.Siblings[1], i - x - 1);
        }//Parece natural que este metodo sean de instancia
        public static Subject Copy(Subject node)
        {
            Subject sub = new Subject(node.Type);

            if (node.Type != TypeNode.Char)
                sub.AppendChild(Copy(node.Siblings[0]), 0, true);
            else sub.Symbol = node.Symbol;

            if (node.Type == TypeNode.Concat || node.Type == TypeNode.Union)
                sub.AppendChild(Copy(node.Siblings[1]), 1, true);

            return sub;
        }//Parece natural que este metodo sean de instancia

        //Genetic Methods
        public static void Mutate(Subject node, float chance, char[] abc)
        {
            Random rnd = new Random();
            int size = Amount(node) - Asterics(node);
            int x = (int)(chance * size);
            foreach (Subject s in node.PreOrder())
            {
                if (rnd.Next(size) <= x)
                {
                    if (s.Type == TypeNode.Power)
                    {
                        int a = s.IsLeftSon ? 0 : 1;
                        s.Parent.AppendChild(s.Siblings[0], a, false);//Elimina el asterisco
                    }
                    else
                    {
                        if (s.Type == TypeNode.Union)
                            s.Type = TypeNode.Concat;
                        else if (s.Type == TypeNode.Concat)
                            s.Type = TypeNode.Union;
                        else
                        {
                            s.Type = TypeNode.Char;
                            s.Symbol = abc[rnd.Next(abc.Length)];
                        }
                    }
                }
            }
        }
        public static Subject[] Crossover(Subject sub1, Subject sub2)
        {
            Random rnd = new Random();
            Random rnd1 = new Random();
            int pos1 = rnd.Next(Amount(sub1));
            int pos2 = rnd.Next(Amount(sub2));

            Subject sib1 = Copy(sub1);
            Subject sib2 = Copy(sub2);

            Subject cr1 = Select(sib1, pos1);
            Subject cr2 = Select(sib2, pos2);

            Subject pr1 = cr1.Parent;
            Subject pr2 = cr2.Parent;

            bool cr1Left = cr1.IsLeftSon;
            bool cr2Left = cr2.IsLeftSon;

            if (cr1Left)
                pr1.AppendChild(cr2, 0, false);
            else pr1.AppendChild(cr2, 1, false);

            if (cr2Left)
                pr2.AppendChild(cr1, 0, false);
            else pr2.AppendChild(cr1, 1, false);

            return new Subject[] { sib1, sib2 };
        }

        //Constructors
        public Subject(TypeNode type)
        {
            AmountP = type == TypeNode.Root ? 0 : 1;
            Type = type;
            Siblings = new Subject[2];
        }
        public Subject(char sym) : this(TypeNode.Char)
        {
            Symbol = sym;
        }

        //Methods
        public void AppendChild(Subject node, int i, bool single)
        {
            node.IsLeftSon = i == 0;
            Siblings[i] = node;
            node.Parent = this;
            Update(!single);
        }
        public Subject CopyNode()
        {
            return new Subject(Type);
        }
        public IEnumerable<Subject> PreOrder()
        {
            if (Type == TypeNode.Root)
                foreach (Subject s in Siblings[0].PreOrder())
                    yield return s;
            else
            {
                yield return this;
                if (Type != TypeNode.Char)
                {
                    foreach (Subject s in Siblings[0].PreOrder())
                        yield return s;
                    if (Type != TypeNode.Power)
                        foreach (Subject s in Siblings[1].PreOrder())
                            yield return s;
                }
            }
        }
        public string Regex()
        {
            string left = "", right = "";
            string ret = "";
            switch (Type)
            {
                case TypeNode.Root:
                    ret = Siblings[0].Regex();
                    break;
                case TypeNode.Union:
                    left = Siblings[0].Regex();
                    right = Siblings[1].Regex();
                    ret = "(" + left + "|" + right + ")";
                    break;
                case TypeNode.Concat:
                    left = Siblings[0].Regex();
                    right = Siblings[1].Regex();
                    ret = "(" + left + right + ")";
                    break;
                case TypeNode.Power:
                    left = Siblings[0].Regex();
                    ret = "(" + left + ")" + "*";
                    break;
                case TypeNode.Char:
                    ret = Symbol.ToString();
                    break;
                default:
                    break;
            }
            return ret;
        }
        public static IEnumerable<Subject> InOrder(Subject node)
        {
            Subject current = node;
            Queue<Subject> q = new Queue<Subject>();
            if (node.Type == TypeNode.Root)
                current = node.Siblings[0];
            q.Enqueue(current);
            while (q.Count != 0)
            {
                Subject s = q.Dequeue();
                yield return s;
                if (s.Siblings[0] != null)
                {
                    q.Enqueue(s.Siblings[0]);
                    if (s.Siblings[1] != null)
                        q.Enqueue(s.Siblings[1]);
                }
            }

        }
        public NDA GetNDA(char[] abc)
        {
            if (Type == TypeNode.Root)
            {
                if (Siblings[0] != null) return Siblings[0].GetNDA(abc);
                else return null;
            }
            NDA ret = new NDA();
            GetNDA(0, out ret, abc);
            return ret;
        }
        public void Factorize()
        {
            switch (Type)
            {
                case TypeNode.Root:
                    if (Siblings[0] != null)
                        Siblings[0].Factorize();
                    Update(false);
                    break;
                case TypeNode.Union:
                    Siblings[0].Factorize();
                    Siblings[1].Factorize();
                    ExecFactorize();
                    Update(false);
                    break;
                case TypeNode.Concat:
                    Siblings[0].Factorize();
                    Siblings[1].Factorize();
                    ExecFactorize();
                    Update(false);
                    break;
                case TypeNode.Power:
                    Siblings[0].Factorize();
                    ExecFactorize();
                    Update(false);
                    break;
                case TypeNode.Char:
                    break;
                default:
                    break;
            }
        }
        public string GetWord(int factor, Random rnd)
        {
            string ret = "";
            switch (Type)
            {
                case TypeNode.Root:
                    if (Siblings[0] != null) return Siblings[0].GetWord(factor, rnd);
                    return ret;
                case TypeNode.Union:
                    string left = Siblings[0].GetWord(factor, rnd);
                    string right = Siblings[1].GetWord(factor, rnd);
                    if (rnd.Next(2) == 0)
                        return left;
                    return right;
                case TypeNode.Concat:
                    string left1 = Siblings[0].GetWord(factor, rnd);
                    string right1 = Siblings[1].GetWord(factor, rnd);
                    return left1 + right1;
                case TypeNode.Power:
                    string pow = "";
                    int k = rnd.Next(factor);
                    for (int i = 0; i < k; i++)
                        pow += Siblings[0].GetWord(factor, rnd);
                    return pow;
                case TypeNode.Char:
                    return Symbol.ToString();
                default:
                    return ret;
            }
        }

        //Private Methods
        int GetNDA(int startIndex, out NDA result, char[] abc)
        {
            result = null;//It is always gonna be assigned later
            NDA left = new NDA();
            NDA right = new NDA();
            int i = startIndex;
            switch (Type)
            {
                case TypeNode.Root:
                    break;
                case TypeNode.Union:
                    startIndex = Siblings[0].GetNDA(startIndex, out left, abc);
                    startIndex = Siblings[1].GetNDA(startIndex, out right, abc);
                    result = GetUnionNDA(startIndex, left, right, abc);
                    startIndex += 2;
                    break;
                case TypeNode.Concat:
                    startIndex = Siblings[0].GetNDA(startIndex, out left, abc);
                    startIndex = Siblings[1].GetNDA(startIndex, out right, abc);
                    result = GetConcatNDA(startIndex, left, right);
                    //All but this add two new states
                    break;
                case TypeNode.Power:
                    startIndex = Siblings[0].GetNDA(startIndex, out left, abc);
                    result = GetPowerNDA(startIndex, left, abc);
                    startIndex += 2;
                    break;
                case TypeNode.Char:
                    result = GetCharNDA(startIndex, abc);
                    startIndex += 2;
                    break;
                default:
                    break;
            }
            return startIndex;
        }
        NDA GetCharNDA(int startIndex, char[] abc)
        {
            State st0 = new State(startIndex, false, true);
            State st1 = new State(startIndex + 1, true, false);
            NDA result = new NDA(new List<State>() { st0, st1 }, abc);
            result.AddTransition(st0, Symbol, st1);
            return result;
        }
        NDA GetUnionNDA(int startIndex, NDA left, NDA right, char[] abc)
        {
            State iniL = left.InitialState;
            iniL.Initial = false;
            State finL = left.FinalState;
            finL.Final = false;
            State iniR = right.InitialState;
            iniR.Initial = false;
            State finR = right.FinalState;
            finR.Final = false;

            State st0 = new State(startIndex, false, true);
            State st1 = new State(startIndex + 1, true, false);

            NDA result = new NDA(new List<State>() { st0, st1 }, abc);

            result.AddEpsTransition(st0, iniL);
            result.AddEpsTransition(st0, iniR);
            result.AddEpsTransition(finL, st1);
            result.AddEpsTransition(finR, st1);

            return result;
        }
        NDA GetConcatNDA(int startIndex, NDA left, NDA right)
        {
            State fin = left.FinalState;
            fin.Final = false;
            State ini = right.InitialState;
            ini.Initial = false;
            fin.AddEpsMove(ini);
            NDA result = left;
            result.FinalState = right.FinalState;
            return result;
        }
        NDA GetPowerNDA(int startIndex, NDA son, char[] abc)
        {
            State ini = son.InitialState;
            ini.Initial = false;
            State fin = son.FinalState;
            fin.Final = false;

            State st0 = new State(startIndex, false, true);
            State st1 = new State(startIndex + 1, true, false);

            NDA result = new NDA(new List<State>() { st0, st1 }, abc);
            result.AddEpsTransition(st0, ini);
            result.AddEpsTransition(st0, st1);
            result.AddEpsTransition(fin, ini);
            result.AddEpsTransition(fin, st1);

            return result;
        }
        void Update(bool parent)
        {
            if (Type != TypeNode.Root)
            {
                AmountP = Amount(Siblings[0]) + Amount(Siblings[1]) + 1;
                HeightP = Math.Max(Height(Siblings[0]), Height(Siblings[1])) + 1;

                int x = 0;
                if (Type == TypeNode.Power)
                    x = 1;
                AstericsP = Asterics(Siblings[0]) + Asterics(Siblings[1]) + x;

                if (parent)
                    Parent.Update(parent);
            }
            else
            {
                AmountP = Amount(Siblings[0]);
                HeightP = Height(Siblings[0]);
                AstericsP = Asterics(Siblings[0]);
            }
        }
        void Factorize1()//a|a--->a
        {
            if (Type == TypeNode.Union
                && Siblings[0].Type == TypeNode.Char
                && Siblings[0].Type == TypeNode.Char
                && Siblings[0].Symbol == Siblings[1].Symbol)
            {
                Type = TypeNode.Char;
                Symbol = Siblings[0].Symbol;
                HeightP = 0;
                AmountP = 1;
                AstericsP = 0;
                Siblings[0] = null;
                Siblings[1] = null;
            }
        }
        void Factorize2()//(*)*--->*
        {
            if (Type == TypeNode.Power && Siblings[0].Type == TypeNode.Power)
            {
                AppendChild(Siblings[0].Siblings[0], 0, true);
            }
        }
        void Factorize3()//(x*|.y*)*--->(x|y)*
        {
            if (Type == TypeNode.Power
                && (Siblings[0].Type == TypeNode.Concat || Siblings[0].Type == TypeNode.Union)
                && Siblings[0].Siblings[0].Type == TypeNode.Power
                && Siblings[0].Siblings[1].Type == TypeNode.Power)
            {
                Siblings[0].Type = TypeNode.Union;
                Siblings[0].AppendChild(Siblings[0].Siblings[0].Siblings[0], 0, true);
                Siblings[0].AppendChild(Siblings[0].Siblings[1].Siblings[0], 1, true);
            }
        }
        void Factorize4()//ax|ay--->a(x|y)
        {
            if (Type == TypeNode.Union
                && Siblings[0].Type == TypeNode.Concat
                && Siblings[1].Type == TypeNode.Concat)
            {
                Subject a = Siblings[0].Siblings[0];
                Subject b = Siblings[0].Siblings[1];
                Subject c = Siblings[1].Siblings[0];
                Subject d = Siblings[1].Siblings[1];

                if (a.Type == TypeNode.Char && c.Type == TypeNode.Char && a.Symbol == c.Symbol)
                {
                    Siblings[1].Type = TypeNode.Union;
                    Siblings[1].AppendChild(b, 0, true);
                    Type = TypeNode.Concat;
                    AppendChild(a, 0, true);
                }
                else if (b.Type == TypeNode.Char && d.Type == TypeNode.Char && b.Symbol == d.Symbol)
                {
                    Siblings[0].Type = TypeNode.Union;
                    Siblings[0].AppendChild(c, 1, true);
                    Type = TypeNode.Concat;
                    AppendChild(b, 1, true);
                }
            }
        }
        void Factorize5() //b|(a|b)=a|b
        {

        }
        void Factorize6() //a*|a=a*
        {
            if (Type == TypeNode.Union)
            {
                Subject a = Siblings[0];
                Subject b = Siblings[1];
                if (a.Type == TypeNode.Power
                    && b.Type == TypeNode.Char
                    && a.Siblings[0].Type == TypeNode.Char
                    && a.Siblings[0].Symbol == b.Symbol)
                {
                    int x = IsLeftSon ? 0 : 1;
                    Parent.AppendChild(a, x, true);
                }
                else if (b.Type == TypeNode.Power
                    && a.Type == TypeNode.Char
                    && b.Siblings[0].Type == TypeNode.Char
                    && b.Siblings[0].Symbol == a.Symbol)
                {
                    int x = IsLeftSon ? 0 : 1;
                    Parent.AppendChild(b, x, true);
                }
            }
        }
        void ExecFactorize()
        {
            Factorize1();
            Factorize2();
            Factorize3();
            Factorize4();
            Factorize5();
            Factorize6();
            //Update(true);
        }
    }
    public class SubjectInPopulation : IComparable<SubjectInPopulation>
    {
        //Public Properties
        public Subject Subject { get; set; }
        public float Fitness { get; set; }
        NDA Automaton { get; set; }

        //Constructors
        public SubjectInPopulation(Subject sub, float fitness)
        { Subject = sub; Fitness = fitness; }
        public SubjectInPopulation(Subject sub) : this(sub, 0)
        { }

        //Operators
        public override bool Equals(object obj)
        {
            if (obj is SubjectInPopulation)
                return (SubjectInPopulation)obj == this;
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) == 0;
        }
        public static bool operator !=(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) != 0;
        }
        public static bool operator <(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) < 0;
        }
        public static bool operator <=(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) <= 0;
        }
        public static bool operator >(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) > 0;
        }
        public static bool operator >=(SubjectInPopulation st1, SubjectInPopulation st2)
        {
            return st1.CompareTo(st2) >= 0;
        }

        //Methods
        public int CompareTo(SubjectInPopulation other)
        {
            //return Fitness.CompareTo(other.Fitness);
            Subject s1 = Subject;
            Subject s2 = other.Subject;
            if (Fitness != other.Fitness) return Fitness.CompareTo(other.Fitness);
            else if (Subject.Amount(s1) != Subject.Amount(s2) || Subject.Asterics(s1) != Subject.Asterics(s2))
                return CompareNodeByNode(s1, s2);
            else return 0;
        }
        public float CalculateFitness(ICollection<Tuple<string, WordType>> words, EvaluationType type, char[] abc, int maxAmount, int maxAsterics)
        {
            float sum = 0;
            float a = type.AcceptedValue;
            float s = type.SubsequenceValue;
            float d = type.DeniedValue;

            if (Subject.Amount(Subject) > maxAmount)
                return -1;
            if (Subject.Asterics(Subject) > maxAsterics)
                return -1;
            Automaton = Subject.GetNDA(abc);
            Regex r = new Regex(Subject.Regex());

            foreach (Tuple<string, WordType> tuple in words)
            {
                WordType mark = tuple.Item2;
                string word = tuple.Item1;
                //int fitS = CalculateFitness(word, true);
                int fitAD = CalculateFitness(r, word, false);
                switch (mark)
                {
                    //case WordType.SAW:
                    //    sum += s * fitS;
                    //    break;
                    case WordType.AW:
                        sum += a * fitAD;
                        break;
                    case WordType.DW:
                        sum -= d * fitAD;
                        break;
                    default:
                        break;
                }
            }
            
            return Fitness = ((float)sum / words.Count);
        }

        //Private Methods
        static int CompareNodeByNode(Subject s1, Subject s2)
        {
            if (s1 == null) return (s2 == null) ? 0 : -1;
            if (s2 == null) return 1;

            if (s1.Type == s2.Type)
            {
                if (s1.Type == TypeNode.Root || s1.Type == TypeNode.Power)
                    return CompareNodeByNode(s1.Siblings[0], s2.Siblings[0]);
                else if (s1.Type == TypeNode.Concat || s1.Type == TypeNode.Union)
                {
                    int h1 = CompareNodeByNode(s1.Siblings[0], s2.Siblings[0]);
                    if (h1 == 0)
                        return CompareNodeByNode(s1.Siblings[1], s2.Siblings[1]);
                    else
                        return h1;
                }
                else
                    return s1.Symbol.CompareTo(s2.Symbol);
            }
            else
                return s1.Type.CompareTo(s2.Type);
        }
        int CalculateFitness(Regex r, string word, bool subseq)
        {
            if (subseq)
                return Automaton.Belongs(word) ? 1 : 0;
            else return Automaton.Belongs(word) ? 1 : -1;


            //return r.IsMatch(word) ? 1 : -1;
        }
    }
    public class FitnessComparison : IComparer<SubjectInPopulation>
    {
        public int Compare(SubjectInPopulation x, SubjectInPopulation y)
        {
            return x.Fitness.CompareTo(y.Fitness);
        }
    }

    public class NDA
    {
        //Instance Properties
        public State InitialState { get; set; }
        public State FinalState { get; set; }

        //Constructor
        public NDA(List<State> states, int initial, int final, char[] abc)
        {
            foreach (State s in states)
                foreach (char a in abc)
                    s.Move.Add(a, new SortedSet<State>());
            InitialState = states[initial];
            FinalState = states[final];
            InitialState.Initial = true;
            FinalState.Final = true;
        }
        public NDA(List<State> states, char[] abc) : this(states, 0, states.Count - 1, abc) { }
        public NDA() { }

        //Private Methods
        SortedSet<State> EpsClosure(SortedSet<State> sts)
        {
            SortedSet<State> analyzed = new SortedSet<State>();
            Queue<State> q = new Queue<State>();
            foreach (State s in sts)
                q.Enqueue(s);
            while (q.Count != 0)
            {
                State current = q.Dequeue();
                if (!analyzed.Contains(current))
                {
                    analyzed.Add(current);
                    foreach (State s in current.EpsTransitions)
                        q.Enqueue(s);
                }
            }
            return analyzed;
        }
        SortedSet<State> Move(SortedSet<State> sts, char x)
        {
            SortedSet<State> ret = new SortedSet<State>();
            foreach (State s in sts)
                foreach (State st in s.Move[x])
                    ret.Add(st);
            return ret;
        }
        SortedSet<State> EpsMove(SortedSet<State> sts, char x)
        {
            return EpsClosure(Move(sts, x));
        }

        //Methods
        public bool Belongs(string word)
        {
            SortedSet<State> currentSts = EpsClosure(new SortedSet<State>() { InitialState });
            for (int i = 0; i < word.Length; i++)
            {
                char x = word[i];
                currentSts = EpsMove(currentSts, x);
                if (currentSts.Count == 0) return false;
            }
            foreach (State st in currentSts)
                if (st.Final) return true;
            return false;
        }
        public bool AddTransition(State st1, char x, State st2)
        {
            return AddTransition(st1, x, new SortedSet<State> { st2 });
        }
        public bool AddTransition(State st, char x, SortedSet<State> sts)
        {
            bool ret = true;
            foreach (State a in sts)
                ret &= st.AddMove(x, a);
            return ret;
        }
        public bool AddEpsTransition(State st1, State st2)
        {
            return AddEpsTransition(st1, new SortedSet<State> { st2 });
        }
        public bool AddEpsTransition(State st, SortedSet<State> sts)
        {
            bool ret = true;
            foreach (State a in sts)
                ret &= st.AddEpsMove(a);
            return ret;
        }

        public IEnumerable<Tuple<int, char, int>> Transitions(bool all)
        {
            Set<State> analyzed = new Set<State>();
            Queue<State> q = new Queue<State>();
            q.Enqueue(InitialState);
            while (q.Count != 0)
            {
                State current = q.Dequeue();
                if (!analyzed.Find(current))
                {
                    analyzed.Add(current);
                    foreach (char key in current.Move.Keys)
                        foreach (State s in current.Move[key])
                        {
                            yield return new Tuple<int, char, int>(current.Index, key, s.Index);
                            q.Enqueue(s);
                        }
                    foreach (State s in current.EpsTransitions)
                    {
                        if (all || s != current)
                            yield return new Tuple<int, char, int>(current.Index, 'E', s.Index);
                        q.Enqueue(s);
                    }
                }
            }
        }
    }
    public class State : IComparable<State>
    {
        //Instance Properties
        public bool Final { get; set; }
        public bool Initial { get; set; }
        public int Index { get; set; }
        public Dictionary<char, SortedSet<State>> Move { get; set; }
        public SortedSet<State> EpsTransitions { get; set; }

        //Constructors
        public State(int index, bool final, bool initial)
        {
            Index = index;
            Final = final;
            Initial = initial;
            Move = new Dictionary<char, SortedSet<State>>();
            EpsTransitions = new SortedSet<State>() { this };
        }
        public State(int index) : this(index, false, false) { }

        //Methods
        public bool AddMove(char a, State st)
        {
            if (!Move.ContainsKey(a))
                Move.Add(a, new SortedSet<State> { st });
            return Move[a].Add(st);
        }
        public bool AddEpsMove(State st)
        {
            return EpsTransitions.Add(st);
        }
        public int CompareTo(State st)
        {
            return Index.CompareTo(st.Index);
        }
    }
    public class Set<T> where T : IComparable<T>
    {
        //Insatnce Properties

        bool root, isLeftSon;
        Set<T> Parent { get; set; }
        Set<T> LeftSon { get; set; }
        Set<T> RightSon { get; set; }

        //Constructors
        public Set() : this(true) { }
        public Set(ICollection<T> list)
        {
            foreach (T val in list)
                Add(val);
        }
        private Set(bool root)
        {
            this.root = root;
        }
        private Set(T value) : this(value, false) { }
        private Set(T value, bool root)
        {
            Height = -1;
            Value = value;
            this.root = root;
        }

        //Public Properties
        public T Value { get; private set; }
        public int Height { get; private set; }
        public int Count { get; private set; }
        public int Balance
        {
            get { return root ? 0 : GetHeight(RightSon) - GetHeight(LeftSon); }
        }

        //Static Methods
        static int GetHeight(Set<T> node) { return node == null ? -1 : node.Height; }
        static int GetCount(Set<T> node) { return node == null ? 0 : node.Count; }
        static void RotateLeft(Set<T> node)
        {
            node.isLeftSon = node.Parent.isLeftSon;
            node.Parent.isLeftSon = true;
            if (node.LeftSon != null)
                node.LeftSon.isLeftSon = false;

            node.Parent.RightSon = node.LeftSon;
            if (node.isLeftSon)
                node.Parent.Parent.LeftSon = node;
            else
                node.Parent.Parent.RightSon = node;
            node.LeftSon = node.Parent;
            node.Parent = node.Parent.Parent;
            node.LeftSon.Parent = node;
            if (node.LeftSon.RightSon != null)
                node.LeftSon.RightSon.Parent = node.LeftSon;

            node.root = node.LeftSon.root;
            node.LeftSon.root = false;

            Update(node.LeftSon, node);

        }
        static void RotateRight(Set<T> node)
        {
            node.isLeftSon = node.Parent.isLeftSon;
            node.Parent.isLeftSon = false;
            if (node.RightSon != null)
                node.RightSon.isLeftSon = true;

            node.Parent.LeftSon = node.RightSon;
            if (node.isLeftSon)
                node.Parent.Parent.LeftSon = node;
            else
                node.Parent.Parent.RightSon = node;
            node.RightSon = node.Parent;
            node.Parent = node.Parent.Parent;
            node.RightSon.Parent = node;
            if (node.RightSon.LeftSon != null)
                node.RightSon.LeftSon.Parent = node.RightSon;

            node.root = node.RightSon.root;
            node.RightSon.root = false;

            Update(node.RightSon, node);

        }
        static void Update(params Set<T>[] nodes)
        {
            foreach (Set<T> node in nodes)
            {
                if (node.root)
                {
                    if (node.LeftSon != null)
                    {
                        node.Count = GetCount(node.LeftSon.LeftSon) + GetCount(node.LeftSon.RightSon) + 1;
                        node.Height = Math.Max(GetHeight(node.LeftSon.LeftSon), GetHeight(node.LeftSon.RightSon)) + 1;
                    }
                    else
                    {
                        node.Count = 0;
                        node.Height = 0;
                    }
                }
                else
                {
                    node.Count = GetCount(node.LeftSon) + GetCount(node.RightSon) + 1;
                    node.Height = Math.Max(GetHeight(node.LeftSon), GetHeight(node.RightSon)) + 1;
                }
            }
        }
        static void Append(Set<T> node, Set<T> item, bool left)
        {
            if (left)
                node.LeftSon = item;
            else
                node.RightSon = item;

            if (item != null)
            {
                item.isLeftSon = left;
                item.Parent = node;
            }
        }
        static bool Insert(Set<T> node, Set<T> item)
        {
            if (node.root)
            {
                if (node.LeftSon == null)
                {
                    Append(node, item, true);
                    return true;
                }
                else return Insert(node.LeftSon, item);
            }

            else
            {
                int x = item.Value.CompareTo(node.Value);
                if (x < 0)
                {
                    if (node.LeftSon != null) return Insert(node.LeftSon, item);
                    else
                    {
                        Append(node, item, true);
                        return true;
                    }
                }
                else if (x > 0)
                {
                    if (node.RightSon != null) return Insert(node.RightSon, item);
                    else
                    {
                        Append(node, item, false);
                        return true;
                    }
                }
                else return false;//Ya esta en el arbol
            }
        }
        static bool Insert(Set<T> node, Set<T> item, IComparer<T> comparer)
        {
            if (node.root)
            {
                if (node.LeftSon == null)
                {
                    Append(node, item, true);
                    return true;
                }
                else return Insert(node.LeftSon, item, comparer);
            }

            else
            {
                int x = comparer.Compare(item.Value, node.Value);
                if (x < 0)
                {
                    if (node.LeftSon != null) return Insert(node.LeftSon, item, comparer);
                    else
                    {
                        Append(node, item, true);
                        return true;
                    }
                }
                else if (x > 0)
                {
                    if (node.RightSon != null) return Insert(node.RightSon, item, comparer);
                    else
                    {
                        Append(node, item, false);
                        return true;
                    }
                }
                else return false;//Ya esta en el arbol
            }
        }
        static bool Delete(Set<T> node, T value, out Set<T> parent)
        {
            parent = null; ;
            Set<T> item = new Set<T>(false);
            bool ret = Find(node, value, out item);
            if (ret)
            {
                if (item.LeftSon == null && item.RightSon == null)
                {
                    parent = item.Parent;
                    Append(parent, null, item.isLeftSon);
                }
                else if (item.LeftSon != null && item.RightSon != null)
                {
                    Set<T> m = GetMin(item.RightSon);//Minimun of greaters
                    Swap(item, m);
                    parent = m.Parent;
                    Append(parent, m.RightSon, m.isLeftSon);
                }
                else if (item.LeftSon != null)
                {
                    parent = item.Parent;
                    Append(parent, item.LeftSon, item.isLeftSon);
                }
                else
                {
                    parent = item.Parent;
                    Append(parent, item.RightSon, item.isLeftSon);
                }
                Update(parent);
            }
            return ret;
        }
        static bool Delete(Set<T> node, T value, out Set<T> parent, IComparer<T> comparer)
        {
            parent = null; ;
            Set<T> item = new Set<T>(false);
            bool ret = Find(node, value, out item, comparer);
            if (ret)
            {
                if (item.LeftSon == null && item.RightSon == null)
                {
                    parent = item.Parent;
                    Append(parent, null, item.isLeftSon);
                }
                else if (item.LeftSon != null && item.RightSon != null)
                {
                    Set<T> m = GetMin(item.RightSon);//Minimun of greaters
                    Swap(item, m);
                    parent = m.Parent;
                    Append(parent, m.RightSon, m.isLeftSon);
                }
                else if (item.LeftSon != null)
                {
                    parent = item.Parent;
                    Append(parent, item.LeftSon, item.isLeftSon);
                }
                else
                {
                    parent = item.Parent;
                    Append(parent, item.RightSon, item.isLeftSon);
                }
                Update(parent);
            }
            return ret;
        }
        static void FixAfterInsert(Set<T> current)
        {
            while (!current.root)
            {
                Update(current);
                int B = current.Balance;
                if (Math.Abs(B) == 2)//Balance ++ or --
                {
                    FixUnbalance(current);
                    current = current.Parent.Parent;
                }
                else
                    current = current.Parent;
            }
            Update(current);//updating root;
        }
        static void FixAfterDelete(Set<T> current)
        {
            while (!current.root)
            {
                Update(current);
                int B = current.Balance;
                if (Math.Abs(B) == 2)
                {
                    FixUnbalance(current);//Not handle ++,. nor --,. unbalances then...
                    B = current.Balance;
                    if (B == 2 && current.RightSon.Balance == 0)
                        RotateLeft(current.RightSon);
                    else if (B == -2 && current.LeftSon.Balance == 0)
                        RotateRight(current.LeftSon);
                    current = current.Parent.Parent;
                }
                else
                    current = current.Parent;
            }
            Update(current);//updating root;
        }
        static void FixUnbalance(Set<T> node)
        {
            int B = node.Balance;
            if (B == 2)//++
            {
                if (node.RightSon.Balance == 1)//++,+
                    RotateLeft(node.RightSon);
                else if (node.RightSon.Balance == -1)//++,-
                {
                    RotateRight(node.RightSon.LeftSon);
                    RotateLeft(node.RightSon);
                }
            }
            else//--
            {
                if (node.LeftSon.Balance == -1)//--,-
                    RotateRight(node.LeftSon);
                else if (node.LeftSon.Balance == 1)//--,+
                {
                    RotateLeft(node.LeftSon.RightSon);
                    RotateRight(node.LeftSon);
                }
            }
        }
        static void Swap(Set<T> a, Set<T> b)
        {
            T temp = a.Value;
            a.Value = b.Value;
            b.Value = temp;
        }
        static Set<T> GetMin(Set<T> node)
        {
            Set<T> current = node;
            while (current.LeftSon != null)
                current = current.LeftSon;
            return current;
        }
        static Set<T> GetMax(Set<T> node)
        {
            Set<T> current = node;
            while (node.RightSon != null)
                current = current.RightSon;
            return current;
        }
        static bool Find(Set<T> node, T value, out Set<T> item)
        {
            if (node.root)
                item = node.LeftSon;
            else item = node;
            while (item != null)
            {
                int x = value.CompareTo(item.Value);
                if (x == 0) return true;
                else if (x < 0)
                    item = item.LeftSon;
                else item = item.RightSon;

            }
            return false;
        }
        static bool Find(Set<T> node, T value, out Set<T> item, IComparer<T> comparer)
        {
            if (node.root)
                item = node.LeftSon;
            else item = node;
            while (item != null)
            {
                int x = comparer.Compare(value, item.Value);
                if (x == 0) return true;
                else if (x < 0)
                    item = item.LeftSon;
                else item = item.RightSon;

            }
            return false;
        }

        //Methods
        public bool Find(T value)
        {
            if (root)
                return LeftSon != null ? LeftSon.Find(value) : false;

            int x = value.CompareTo(Value);
            return ((root || x < 0) && LeftSon != null && LeftSon.Find(value)) || (x > 0 && RightSon != null && RightSon.Find(value)) || x == 0;
        }//Tested
        public bool Find(T value, IComparer<T> comparer)
        {
            if (root)
                return LeftSon != null ? LeftSon.Find(value) : false;

            int x = comparer.Compare(value, Value);
            return ((root || x < 0) && LeftSon != null && LeftSon.Find(value, comparer)) || (x > 0 && RightSon != null && RightSon.Find(value, comparer)) || x == 0;
        }
        public int Rank(T value)
        {
            int x = value.CompareTo(Value);
            if (root || x < 0) return LeftSon == null ? 0 : LeftSon.Rank(value);
            else if (x > 0) return 1 + GetCount(LeftSon) + ((RightSon == null) ? 0 : RightSon.Rank(value));
            else return GetCount(LeftSon);
        }//Tested
        public int Rank(T value, IComparer<T> comparer)
        {
            int x = comparer.Compare(value, Value);
            if (root || x < 0) return LeftSon == null ? 0 : LeftSon.Rank(value, comparer);
            else if (x > 0) return 1 + GetCount(LeftSon) + ((RightSon == null) ? 0 : RightSon.Rank(value, comparer));
            else return GetCount(LeftSon);
        }
        T Select(int index)
        {
            if (root)
            {
                if (LeftSon != null)
                    return LeftSon.Select(index);
                else throw new IndexOutOfRangeException();
            }
            if (index >= Count) throw new IndexOutOfRangeException();
            int x = index - GetCount(LeftSon);
            if (x < 0) return LeftSon.Select(index);
            else if (x > 0) return RightSon.Select(index - GetCount(LeftSon) - 1);
            else return Value;
        }
        T Select(int index, IComparer<T> comparer)
        {
            if (root)
            {
                if (LeftSon != null)
                    return LeftSon.Select(index, comparer);
                else throw new IndexOutOfRangeException();
            }
            if (index >= Count) throw new IndexOutOfRangeException();
            int x = index - GetCount(LeftSon);
            if (x < 0) return LeftSon.Select(index);
            else if (x > 0) return RightSon.Select(index - GetCount(LeftSon) - 1, comparer);
            else return Value;
        }
        public T this[int i]
        {
            get { return Select(i); }
        }//Tested
        public T this[int i, IComparer<T> comparer]
        {
            get { return Select(i, comparer); }
        }
        public bool Add(T value)
        {
            Set<T> item = new Set<T>(value);
            bool ret = Insert(this, item);
            if (ret)
                FixAfterInsert(item);
            return ret;
        }//Tested
        public bool Add(T value, IComparer<T> comparer)
        {
            Set<T> item = new Set<T>(value);
            bool ret = Insert(this, item, comparer);
            if (ret)
                FixAfterInsert(item);
            return ret;
        }
        public bool Remove(T value)
        {
            Set<T> parent = new Set<T>(false);
            Set<T> current = new Set<T>(false);
            bool ret = Delete(this, value, out parent);
            if (ret)
                FixAfterDelete(parent);
            return ret;
        }//Tested
        public bool Remove(T value, IComparer<T> comparer)
        {
            Set<T> parent = new Set<T>(false);
            Set<T> current = new Set<T>(false);
            bool ret = Delete(this, value, out parent, comparer);
            if (ret)
                FixAfterDelete(parent);
            return ret;
        }
        public void RemoveAt(int i)
        {
            Remove(Select(i));
        }//Tested
        public void RemoveAt(int i, IComparer<T> comparer)
        {
            Remove(Select(i), comparer);
        }
        public void RemoveRange(int index, int size)
        {
            while (size-- > 0)
                RemoveAt(index);
        }//Tested
        public void RemoveRange(int index, int size, IComparer<T> comparer)
        {
            while (size-- > 0)
                RemoveAt(index, comparer);
        }

        //IEnumerable
        public IEnumerable<T> PreOrder()
        {
            if (root)
            {
                if (LeftSon != null)
                    foreach (var s in LeftSon.PreOrder()) yield return s;
            }
            else
            {
                if (LeftSon != null)
                    foreach (T val in LeftSon.PreOrder())
                        yield return val;
                yield return Value;
                if (RightSon != null)
                    foreach (T val in RightSon.PreOrder())
                        yield return val;
            }
        }//Tested
        public IEnumerable<T> InOrder()
        {
            Queue<Set<T>> q = new Queue<Set<T>>();
            if (root && LeftSon != null)
                q.Enqueue(LeftSon);
            while (q.Count != 0)
            {
                Set<T> m = q.Dequeue();
                yield return m.Value;
                if (m.LeftSon != null)
                    q.Enqueue(m.LeftSon);
                if (m.RightSon != null)
                    q.Enqueue(m.RightSon);
            }
        }//Tested
    }
    public class Population : IEnumerable<Subject>
    {
        public Set<SubjectInPopulation> population;
        public List<SubjectInPopulation> newSubjects;
        public List<Tuple<string, WordType>> testWords;
        //public Set<string> words;

        char[] abc;
        Random rnd;
        int maxSubjectSize;
        int popSize;

        //Properties
        public int Size { get { return popSize; } set { popSize = value; } }

        public Population(char[] abc) : this(0, 0, abc) { }
        public Population(int popSize, int maxSubjectSize, char[] abc) : this(popSize, maxSubjectSize, abc, new List<Tuple<string, WordType>>()) { }
        public Population(int popSize, int maxSubjectSize, char[] abc, List<Tuple<string, WordType>> testWords)
        {
            this.abc = abc;
            this.popSize = popSize;
            this.maxSubjectSize = maxSubjectSize;
            rnd = new Random();
            population = new Set<SubjectInPopulation>();
            newSubjects = new List<SubjectInPopulation>(2 * popSize);
            this.testWords = testWords;

            for (int i = 0; i < popSize; i++)
            {
                Subject sub = Subject.CreateRandomSubject(maxSubjectSize, abc, rnd);
                SubjectInPopulation subPop = new SubjectInPopulation(sub);
                newSubjects.Add(subPop);
            }
        }

        public void AddWord(Tuple<string, WordType> word)
        {
            testWords.Add(word);
        }
        public void AddWord(ICollection<string> words, WordType type)
        {
            foreach (string w in words)
            {
                testWords.Add(new Tuple<string, WordType>(w, type));
            }
        }
        public void AddWord(ICollection<Tuple<string, WordType>> wordsTyped)
        {
            foreach (Tuple<string, WordType> w in wordsTyped)
            {
                testWords.Add(w);
            }
        }
        public void AlienInvasionStage()
        {
            for (int i = 0; i < popSize; i++)
            {
                Subject sub = Subject.CreateRandomSubject(maxSubjectSize, abc, rnd);
                SubjectInPopulation subPop = new SubjectInPopulation(sub);
                newSubjects.Add(subPop);
            }
        }
        public void SetNewSubjectsFitnessStage(float S, float A, float D, int maxAmount, int maxAsterics)
        {
            foreach (SubjectInPopulation s in newSubjects)
                s.CalculateFitness(testWords, new EvaluationType(S, A, D), abc, maxAmount, maxAsterics);
            foreach (SubjectInPopulation s in newSubjects)
                population.Add(s);
            newSubjects.Clear();
        }
        public void ReducePopulationStage(int size)//Implementar bien
        {
            Random rnd = new Random();
            int toRem = population.Count - size;
            while (toRem > 0)
            {
                float lastFit = population[0].Fitness;
                int wf = WithFitness(lastFit);
                if (wf <= toRem)
                    population.RemoveRange(0, wf);
                else
                {
                    int index = rnd.Next(wf);
                    population.RemoveAt(index);
                }

                toRem = population.Count - size;

            }
            population.RemoveRange(0, population.Count - size);
        }
        int WithFitness(float fitness)
        {
            int ret = 0;
            foreach (SubjectInPopulation s in population.PreOrder())
            {
                if (s.Fitness <= fitness)
                    ret++;
                else break;
            }
            return ret;
        }
        public void CrossoverStage()//Implementar bien
        {
            Random rnd = new Random();
            for (int i = 0; i < population.Count - 1; i++)
            {
                int x = rnd.Next(population.Count);
                int y = rnd.Next(population.Count);
                if (x != y)
                {
                    Subject[] s = Subject.Crossover(population[x].Subject, population[y].Subject);
                    SubjectInPopulation s1 = new SubjectInPopulation(s[0]);
                    SubjectInPopulation s2 = new SubjectInPopulation(s[1]);
                    newSubjects.Add(s1);
                    newSubjects.Add(s2);
                }
            }
        }
        public void MutationStage()//Implementar bien
        {
            for (int i = 0; i < population.Count; i++)
            {
                SubjectInPopulation s = population[i];
                Subject sub = s.Subject;
                Subject sub2 = Subject.Copy(sub);
                Subject.Mutate(sub2, (float)1 / (Subject.Amount(sub2) - Subject.Asterics(sub2)), abc);
                newSubjects.Add(new SubjectInPopulation(sub2));
            }
            //foreach (SubjectInPopulation s in population)
            //{
            //    Subject sub = s.Subject;
            //    Subject sub2 = Subject.Copy(sub);
            //    Subject.Mutate(sub2, (float)1 / (Subject.Amount(sub2) - Subject.Asterics(sub2)), abc);
            //    newSubjects.Add(new SubjectInPopulation(sub2));
            //}
        }
        public void SpecialMutationsStage() { }
        public void FactorizationStage()
        {
            foreach (SubjectInPopulation s in newSubjects)
                s.Subject.Factorize();
        }
        public void RefreshPopulationStage()
        {
            Set<SubjectInPopulation> pop = new Set<SubjectInPopulation>();
            foreach (SubjectInPopulation s in population.PreOrder())
            {
                pop.Add(s, new FitnessComparison());
            }
            population = pop;
        }
        public void RemoveFitnessStage()
        {
            foreach (SubjectInPopulation s in population.PreOrder())
            {
                s.Fitness = 0;
                newSubjects.Add(s);
            }
            population = new Set<SubjectInPopulation>();
        }
        //Private Methods
        static List<SubjectInPopulation> Merge(List<SubjectInPopulation> a, List<SubjectInPopulation> b)
        {
            List<SubjectInPopulation> ret = new List<SubjectInPopulation>(a.Count + b.Count);
            int i = 0;
            int j = 0;
            while (i < a.Count && j < b.Count)
            {
                if (a[i] <= b[j])
                {
                    ret.Add(a[i]);
                    i++;
                }
                else
                {
                    ret.Add(b[j]);
                    j++;
                }
            }
            if (i == a.Count)
                while (j < b.Count)
                {
                    ret.Add(b[j]);
                    j++;
                }
            else
                while (i < a.Count)
                {
                    ret.Add(a[i]);
                    i++;
                }
            return ret;
        }

        //Interface
        public IEnumerator<Subject> GetEnumerator()
        {
            foreach (SubjectInPopulation s in population.PreOrder())
                yield return s.Subject;
        }

        public IEnumerable<SubjectInPopulation> NewSubjects()
        {
            return newSubjects;
        }
        public IEnumerable<Tuple<string, WordType>> TestWords()
        {
            return testWords;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
    public static class PopulationController
    {
        public static void Alice(char[]abc, int questions)
        {
            int popSize = 50;
            int wordSizeFactor = 5;
            int percentageTolerance = 30;
            int maxSize = 20;
            int maxAsterics = 10;
            int alienInvasionRate = 2000000;
            int refreshPopulationRate = 300;
            int mutationRate = 20000000;

            Population pop = new Population(popSize, 10, abc);

            Set<string> accWords = new Set<string>();
            Set<string> denWords = new Set<string>();

            int i = questions;
            int iterations = 0;

            while (i > 0)
            {
                pop.RemoveFitnessStage();
                i -= Questions(pop, accWords, denWords, wordSizeFactor, i);

                int accW = accWords.Count;
                int denW = denWords.Count;

                if (accW < percentageTolerance)
                {
                    if (accW == 0)
                    {
                        iterations = 60;//iteraciones cuando estoy haciendo preguntas y no hay accepted
                    }
                    else
                    {
                        if (accW > 20)
                            iterations = 20;
                        else iterations = 60;

                        if (denW > 200)
                        {
                            Random rnd = new Random();
                            List<Tuple<string, WordType>> words = new List<Tuple<string, WordType>>(questions);
                            foreach (string w in accWords.PreOrder())
                                words.Add(new Tuple<string, WordType>(w, WordType.AW));
                            int k = denWords.Count - 200;
                            while (k-- > 0)
                            {
                                int index = rnd.Next(denWords.Count);
                                words.Add(new Tuple<string, WordType>(denWords[index], WordType.DW));
                                denWords.RemoveAt(index);
                            }
                        }
                    }
                    Exploitation(pop, accWords.Count, denWords.Count, iterations, 50, 40, 10, alienInvasionRate, refreshPopulationRate, mutationRate, 1.5F);
                }
                if (denW < percentageTolerance)
                {
                    pop = new Population(popSize, 10, abc);
                }
            }
            GoGetThem(pop, accWords, denWords, questions);
            SubjectInPopulation s = pop.population[pop.population.Count - 1];
            SubjectInPopulation s1 = pop.population[pop.population.Count - 2];
            SubjectInPopulation s2 = pop.population[pop.population.Count - 3];

            NDA nda1 = s.Subject.GetNDA(abc);
            NDA nda2 = s1.Subject.GetNDA(abc);
            NDA nda3 = s2.Subject.GetNDA(abc);
            for (int k = 0; k < questions; k++)
            {
                string word = Console.ReadLine();
                if (nda1.Belongs(word) || nda2.Belongs(word) || nda3.Belongs(word))
                {
                    Console.WriteLine("yes");
                    Console.Out.Flush();
                }
                else
                {
                    Console.WriteLine("no");
                    Console.Out.Flush();
                }
            }
        }
        static void GoGetThem(Population pop, Set<string> accWords, Set<string> denWords, int questions)
        {
            int accW = accWords.Count;
            int denW = denWords.Count;

            Random rnd = new Random();
            List<Tuple<string, WordType>> words = new List<Tuple<string, WordType>>(questions);
            int acc = 0;
            int den = 0;

            if (denW > 50)
            {
                for (int j = 0; j < 50 && accWords.Count > 0; j++)
                {
                    acc++;
                    int index = rnd.Next(accWords.Count);
                    string word = accWords[index];
                    accWords.RemoveAt(index);
                    words.Add(new Tuple<string, WordType>(word, WordType.AW));
                }
                while (words.Count < 100 && denWords.Count > 0)
                {
                    den++;
                    int index = rnd.Next(denWords.Count);
                    string word = denWords[index];
                    denWords.RemoveAt(index);
                    words.Add(new Tuple<string, WordType>(word, WordType.DW));
                }
            }
            else
            {
                for (int j = 0; j < 50 && denWords.Count > 0; j++)
                {
                    den++;
                    int index = rnd.Next(denWords.Count);
                    string word = denWords[index];
                    denWords.RemoveAt(index);
                    words.Add(new Tuple<string, WordType>(word, WordType.DW));
                }
                while (words.Count < den+50 && accWords.Count > 0)
                {
                    acc++;
                    int index = rnd.Next(accWords.Count);
                    string word = accWords[index];
                    accWords.RemoveAt(index);
                    words.Add(new Tuple<string, WordType>(word, WordType.AW));
                }
            }
            pop.testWords = words;
            pop.RemoveFitnessStage();

            int iterations = 300;
            int refreshPopulationRate = 10;
            int alienInvasionRate = 5;
            int mutationRate = 5;
            int maxSize = 30;
            int popSize = 100;
            int maxAsterics = 5;
            
            float maxFitness;
            if (accW == 0 || denW == 0)
                maxFitness = 0.5F;
            else maxFitness = 0.99F;

            Exploitation(pop, acc, den, iterations, popSize, maxSize, maxAsterics, alienInvasionRate, refreshPopulationRate, mutationRate, maxFitness);
        }

        static void Exploitation(Population pop, int acc, int den, int iterations, int popSize, int maxSize, int maxAsterics, int alienInvasionRate, int refreshPopulationRate, int mutationRate, float maxFitness)
        {
            Random rnd = new Random();
            float accVal = ((float)(acc + den)) / (2 * acc);
            float denVal = ((float)(acc + den)) / (2 * den);

            int psc = 0;
            for (int i = 0; i < iterations; i++)
            {
                if (pop.population.Count>=3&&
                    pop.population[pop.population.Count - 1].Fitness >= maxFitness&&
                    pop.population[pop.population.Count - 2].Fitness >= maxFitness&&
                    pop.population[pop.population.Count - 3].Fitness >= maxFitness)
                    break;
                psc++;
                if (i % refreshPopulationRate == 0)
                {
                    if (i <= iterations / 2)
                    {
                        pop.RefreshPopulationStage();
                        pop.Size = popSize;
                        pop.AlienInvasionStage();
                        pop.SetNewSubjectsFitnessStage(0, accVal, denVal, maxSize, maxAsterics);
                    }
                }
                if (rnd.Next(alienInvasionRate) == 0)
                    pop.AlienInvasionStage();
                if (rnd.Next(mutationRate) == 0)
                    pop.MutationStage();
                pop.FactorizationStage();
                pop.SetNewSubjectsFitnessStage(0, accVal, denVal, maxSize, maxAsterics);
                pop.CrossoverStage();
                pop.FactorizationStage();
                pop.SetNewSubjectsFitnessStage(0, accVal, denVal, maxSize, maxAsterics);
                if (psc % 1 == 0)
                    pop.ReducePopulationStage(popSize);
            }
        }
        static int Questions(Population pop, Set<string> acceptedWords, Set<string> deniedWords, int wordSizeFactor, int remQuestions)
        {
            int ret = 0;
            Random rnd = new Random();
            foreach (SubjectInPopulation sp in pop.NewSubjects())
            {
                int j = 0;
                string word = "";
                WordType type;
                Subject s;
                do
                {
                    j++;
                    s = sp.Subject;
                    word = s.GetWord(wordSizeFactor, rnd);
                }
                while ((acceptedWords.Find(word) || deniedWords.Find(word)||word.Length>20) && j < 50);

                if (ret < remQuestions)//check if there are remWords
                {
                    ret++;
                    if (Belongs(word))
                    {
                        acceptedWords.Add(word);
                        type = WordType.AW;
                    }
                    else
                    {
                        deniedWords.Add(word);
                        type = WordType.DW;
                    }

                    pop.AddWord(new Tuple<string, WordType>(word, type));
                }
                else break;
            }
            return ret;
        }

        static bool Belongs(string word)
        {
            Console.WriteLine(word);
            Console.Out.Flush();
            string answer = Console.ReadLine();
            return answer == "yes";
        }
    }
}
