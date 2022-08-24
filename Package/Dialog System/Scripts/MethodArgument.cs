using System;

[Serializable]
public class MethodArgument
{
    public int ArgType;

    public string StringArg;
    public int IntArg;
    public double DoubleArg;
    public float FloatArg;
    public bool BoolArg;

    // creating new method argument, checking what type of var it is, VERY MESSY DONT LOOK
    public MethodArgument(object arg, int argType)
    {
        ArgType = argType;

        switch (argType)
        {
            // string
            case 0:
                StringArg = arg.ToString();
                break;
            // int
            case 1:
                IntArg = Convert.ToInt32(arg);
                break;
            // double
            case 2:
                DoubleArg = Convert.ToDouble(arg);
                break;
            // float
            case 3:
                FloatArg = float.Parse(arg.ToString());
                break;
            // bool
            case 4:
                BoolArg = arg.ToString() == "True";
                break;
        }
    }

    public object Content()
    {
        switch (ArgType)
        {
            // string
            case 0:
                return StringArg;
            // int
            case 1:
                return IntArg;
            // double
            case 2:
                return DoubleArg;
            // float
            case 3:
                return FloatArg;
            // bool
            case 4:
                return BoolArg;
        }

        return null;
    }

}
    