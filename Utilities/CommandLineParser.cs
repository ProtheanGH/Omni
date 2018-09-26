using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omni.Utilities
{
  class CommandLineParser
  {
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
      AddArgument("help", ArgumentType.Single, ValueType.LooseArgument, "Help command that will log out information for all the supported commandline arguments.");
      AddArgument("config", ArgumentType.Single, ValueType.StringValue, "The configuration file for Omni to load during startup.");
      AddArgument("rows", ArgumentType.Single, ValueType.IntegerValue, "The amount of rows that Omni will configure during startup.", new string[] { "config" });
      AddArgument("columns", ArgumentType.Single, ValueType.IntegerValue, "The amount of columns that Omni will configure during startup.", new string[] { "config" });

      Parse();

      if (_loose_arguments.Contains("help"))
      {
        DisplayHelp();
      }
    }

    public bool GetValue<typename>(string argument, out typename value)
    {
      Argument defined_argument;
      if(true == _defined_arguments.TryGetValue(argument, out defined_argument))
      {
        if(false == IsMatchingType<typename>(defined_argument._value_type))
        {
          // Todo: Better exception message that says the argument it failed for, and the expected type.
          throw new Exception("Data type mismatch.");
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
      value = default(typename);
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
                // Todo: Log out here?
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
              // Todo: Log out exception
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
            // Todo: Log out exception
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
      // Todo: Once the log is created, use the log instead of a string ...
      foreach (KeyValuePair<string, Argument> arg in _defined_arguments)
      {
        message += "\n" + arg.Key + "\t | \t" + arg.Value._value_type.ToString() + "\n";
        message += "\t" + arg.Value._description;
      }

      int debug = 0;
    }
  }
}
