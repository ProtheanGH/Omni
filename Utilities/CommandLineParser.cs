using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni.Utilities
{
  class CommandLineParser
  {
    // --- Arguments ---
    public const string kArgument_Help = "help";
    public const string kArgument_Config = "config";
    public const string kArgument_Rows = "rows";
    public const string kArgument_Columns = "columns";

    // --- Private Variables ---
    private Dictionary<string, Argument> _defined_arguments;
    private Dictionary<string, List<string>> _string_argument_values;
    private Dictionary<string, List<int>> _int_argument_values;
    private Dictionary<string, List<double>> _double_argument_values;
    private List<string> _loose_arguments;

    // --- Private Enums ---
    private enum ArgumentType
    {
      // There is only meant of this Argument. If there were multiple in the commandline, the last one will be used.
      Single,
      // There can be multiple instances of this Argument
      Multiple
    }

    private enum ValueType
    {
      StringValue,
      IntegerValue,
      DoubleValue,
      LooseArgument
    }

    // --- Private Structs ---
    private struct Argument
    {
      public string _name;
      public string _description;
      public ArgumentType _arg_type;
      public ValueType _value_type;
      public string[] _ignore_conditions;

      public Argument(string name, ArgumentType arg_type, ValueType value_type, string[] ignore_conditions, string description)
      {
        _name = name;
        _description = description;
        _arg_type = arg_type;
        _value_type = value_type;
        _ignore_conditions = ignore_conditions;
      }
    }

    // --- Public Interface ---
    public CommandLineParser()
    {
      _defined_arguments = new Dictionary<string, Argument>();
      _string_argument_values = new Dictionary<string, List<string>>();
      _int_argument_values = new Dictionary<string, List<int>>();
      _double_argument_values = new Dictionary<string, List<double>>();
      _loose_arguments = new List<string>();

      // Define Arguments here
      AddArgument(kArgument_Help, ArgumentType.Single, ValueType.LooseArgument, "Help command that will log out information for all the supported commandline arguments.");
      AddArgument(kArgument_Config, ArgumentType.Single, ValueType.StringValue, "The configuration file for Omni to load during startup.");
      AddArgument(kArgument_Rows, ArgumentType.Single, ValueType.IntegerValue, "The amount of rows that Omni will configure during startup.", new string[] { "config" });
      AddArgument(kArgument_Columns, ArgumentType.Single, ValueType.IntegerValue, "The amount of columns that Omni will configure during startup.", new string[] { "config" });

      Parse();

      if (_loose_arguments.Contains("help"))
      {
        DisplayHelp();
      }
    }

    public bool GetValue<typename>(string argument, ref typename value)
    {
      Argument defined_argument;
      if(true == _defined_arguments.TryGetValue(argument, out defined_argument))
      {
        if(false == IsMatchingType<typename>(defined_argument._value_type))
        {
          Logger.WriteLine(Logger.SeverityType.Error, "Data type mismatch while trying to get command line argument: {0}. Expected a type of {1}.", argument, defined_argument._value_type);
          throw new Exception(String.Format("Data type mismatch. {0} expects a type of {1}.", argument, defined_argument._value_type));
        }

        switch (defined_argument._value_type)
        {
          case ValueType.StringValue:
            {
              List<string> values;
              if(true == _string_argument_values.TryGetValue(argument, out values))
              {
                value = (typename) Convert.ChangeType(values[0], typeof(typename));
                return true;
              }
            }
            break;
          case ValueType.IntegerValue:
            {
              List<int> values;
              if (true == _int_argument_values.TryGetValue(argument, out values))
              {
                value = (typename)Convert.ChangeType(values[0], typeof(typename));
                return true;
              }
            }
            break;
          case ValueType.DoubleValue:
            {
              List<double> values;
              if (true == _double_argument_values.TryGetValue(argument, out values))
              {
                value = (typename)Convert.ChangeType(values[0], typeof(typename));
                return true;
              }
            }
            break;
        }
      }

      // Argument wasn't found (either not defined or not present in the commandline)
      return false;
    }

    public bool HasArgument(string argument)
    {
      if(true == _defined_arguments.ContainsKey(argument))
      {
        if (true == _loose_arguments.Contains(argument))
        {
          return true;
        }
        else if (true == _string_argument_values.ContainsKey(argument))
        {
          return true;
        }
        else if (true == _int_argument_values.ContainsKey(argument))
        {
          return true;
        }
        else if (true == _double_argument_values.ContainsKey(argument))
        {
          return true;
        }
      }

      return false;
    }

    // --- Private Interface ---
    private void Parse()
    {
      string[] arguments = Environment.GetCommandLineArgs();

      // Skip the first argument, as it was just the call to the executable
      for(int i = 1; i < arguments.Length; ++i)
      {
        if(arguments[i].StartsWith("-"))
        {
          arguments[i] = arguments[i].TrimStart('-');

          string[] split_command = arguments[i].Split('=');
          Argument command_argument;
          if (true == _defined_arguments.TryGetValue(split_command[0], out command_argument))
          {
            if (split_command.Length > 1)
            {
              ParseArgument(ref command_argument, split_command[1]);
            }
            else
            {
              if(ValueType.LooseArgument == command_argument._value_type)
              {
                if (false == _loose_arguments.Contains(split_command[0]))
                {
                  _loose_arguments.Add(split_command[0]);
                }
              }
              else
              {
                Logger.WriteLine(Logger.SeverityType.Warning, "No value specified for command line argument: {0}. Skipping ...", split_command[0]);
              }
            }
          }
        }
      }
    }

    private void AddArgument(string name, ArgumentType arg_type, ValueType value_type, string description = "", string[] ignore_if_present = null)
    {
      Argument new_arg = new Argument(name, arg_type, value_type, ignore_if_present, description);
      _defined_arguments.Add(name, new_arg);
    }

    private void ParseArgument(ref Argument argument, string value)
    {
      switch(argument._value_type)
      {
        case ValueType.StringValue:
          {
            List<string> values;
            if(true == _string_argument_values.TryGetValue(argument._name, out values))
            {
              if(argument._arg_type != ArgumentType.Single)
              {
                values.Add(value);
              }
            }
            else
            {
              values = new List<string>();
              values.Add(value);
              _string_argument_values.Add(argument._name, values);
            }
          }
          break;
        case ValueType.IntegerValue:
          {
            try
            {
              int int_value = Convert.ToInt32(value);
              List<int> values;
              if (true == _int_argument_values.TryGetValue(argument._name, out values))
              {
                if (argument._arg_type != ArgumentType.Single)
                {
                  values.Add(int_value);
                }
              }
              else
              {
                values = new List<int>();
                values.Add(int_value);
                _int_argument_values.Add(argument._name, values);
              }
            }
            catch (Exception e)
            {
              Logger.WriteLine(Logger.SeverityType.Warning, "Command line arg: {0}, failed to convert value to an integer. Skipping ...", argument._name);
            }
          }
          break;
        case ValueType.DoubleValue:
          try
          {
            double double_value = Convert.ToDouble(value);
            List<double> values;
            if (true == _double_argument_values.TryGetValue(argument._name, out values))
            {
              if (argument._arg_type != ArgumentType.Single)
              {
                values.Add(double_value);
              }
            }
            else
            {
              values = new List<double>();
              values.Add(double_value);
              _double_argument_values.Add(argument._name, values);
            }
          }
          catch (Exception e)
          {
            Logger.WriteLine(Logger.SeverityType.Warning, "Command line arg: {0}, failed to convert value to a double. Skipping ...", argument._name);
          }
          break;
        case ValueType.LooseArgument:
          if(false == _loose_arguments.Contains(argument._name))
          {
            _loose_arguments.Add(argument._name);
          }
          break;
      }
    }

    private bool IsMatchingType<typename>(ValueType type)
    {
      switch (type)
      {
        case ValueType.StringValue:
          return typeof(typename) == typeof(string);
        case ValueType.IntegerValue:
          return typeof(typename) == typeof(int);
        case ValueType.DoubleValue:
          return typeof(typename) == typeof(double);
        default:
          return false;
      }
    }

    private void DisplayHelp()
    {
      string message = "CommandLine Arguments\n";
      message += "--------------------------------\n";
      message += "Omni can be started with any of the following arguments to perform certain actions without the user. Arguments should follow the format of:\n\t-{argument}={value}\n";

      message += "\n\nArgument Types:\n";
      message += "- Single\n";
      message += "\tThere should only be one instance of this argument. If there are more than one instance of this argument, only the first one is applied.\n";
      message += "- Multiple\n";
      message += "\tMultiple instances of this argument is supported.\n";

      message += "\n\nValue Types:\n";
      message += "- StringValue\n";
      message += "\tThe argument is expecting a string value.\n";
      message += "- IntegerValue\n";
      message += "\tThe argument will try to parse it's vaue to an integer, if it can't, then the argument will be skipped.\n";
      message += "- DoubleValue\n";
      message += "\tThe argument will try to parse it's vaue to an double, if it can't, then the argument will be skipped.\n";
      message += "- LooseValue\n";
      message += "\tThe argument is not expecting any value, and will disregard any value, if one was specified. Essentially it's treated as a boolean, if the argument is specified, then TRUE, otherwise FALSE.\n";

      message += "\n\nConditional Arguments:\n";
      message += "Some arguments are conditional, and will be ignored if certain arguments are specified. Check the argument's info to see if there are any arguments that will cause it to be ignored.\n";

      message += "\n\nSupported Arguments:\n";
      foreach (KeyValuePair<string, Argument> arg in _defined_arguments)
      {
        message += "-" + arg.Key + "\t | \t" + arg.Value._value_type.ToString() + "\n";
        message += "\t" + arg.Value._description + "\n";
        message += "\t------\n";
        message += "\t" + "Multiple allowed: " + (arg.Value._arg_type == ArgumentType.Multiple ? "true" : "false") + "\n";
        if (null != arg.Value._ignore_conditions && arg.Value._ignore_conditions.Length > 0)
        {
          message += "\t" + "Ignored if the following argument(s) are present:\n";
          foreach(string ignore_arg in arg.Value._ignore_conditions)
          {
            message += "\t\t-" + ignore_arg + "\n";
          }
        }
      }

      Logger.WriteLine(message);
    }
  }
}
