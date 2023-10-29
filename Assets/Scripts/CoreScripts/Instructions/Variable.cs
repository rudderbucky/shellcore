using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CoreScriptsCondition;
using static CoreScriptsManager;
using static CoreScriptsSequence;

public class Variable : MonoBehaviour
{

    private static bool ValueStartsWith(string val, string strVal)
    {
        val = VariableSensitizeValue(val);
        return val.StartsWith(strVal);
    }

    public static void CompareVals(string val1, string val2, string comp, Condition c, ConditionBlock cb)
    {
        if (!CompareValsHelper(val1, val2, comp, c, cb))
        {
            VariableChangedDelegate del = (x) =>
            {
                if (x == val1 || x == val2 || (x == "MissionStatus(" && (ValueStartsWith(val1, "MissionStatus") || ValueStartsWith(val2, "MissionStatus"))))
                {
                    CompareValsHelper(val1, val2, comp, c, cb);
                }
            };
            CoreScriptsManager.instance.variableChangedDelegates.Add($"{cb.ID}-{c.ID}", del);
        }
    }

    private static string GetComparisonValue(string val)
    {
        val = VariableSensitizeValue(val);
        if (val.StartsWith("MissionStatus("))
        {
            var missionName = val.Trim().Replace("MissionStatus(", "").Replace(")", "").Trim();
            switch (TaskDisplayScript.GetMissionStatus(missionName))
            {
                case Mission.MissionStatus.Inactive:
                    return "inactive";
                case Mission.MissionStatus.Ongoing:
                    return "ongoing";
                case Mission.MissionStatus.Complete:
                    return "complete";
            }
        }
        return val;
    }


    private static bool CompareValsHelper(string val1, string val2, string comp, Condition c, ConditionBlock cb)
    {
        string tmp1 = GetComparisonValue(val1);
        string tmp2 = GetComparisonValue(val2);
        switch (comp)
        {
            case "Eq":
                if (tmp1 == tmp2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
            case "Neq":
                if (tmp1 != tmp2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
            case "Gt":
                var tmpInt1 = int.Parse(tmp1);
                var tmpInt2 = int.Parse(tmp2);
                if (tmpInt1 > tmpInt2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
            case "Lt":
                tmpInt1 = int.Parse(tmp1);
                tmpInt2 = int.Parse(tmp2);
                if (tmpInt1 < tmpInt2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
        }
        return false;
    }




    public static void SetVariable(string variableName, string variableValue)
    {
        var key = "";
        if (variableName.StartsWith("$$$"))
        {
            var names = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames;
            var vals = SaveHandler.instance.GetSave().coreScriptsGlobalVarValues;
            key = variableName.Substring(3);
            var index = names.IndexOf(key);
            if (index >= 0) vals[index] = variableValue;
            else
            {
                names.Add(key);
                vals.Add(variableValue);
            }
        }
        else if (variableName.StartsWith("$$"))
        {
            var dict = CoreScriptsManager.instance.globalVariables;
            key = variableName.Substring(2);
            if (dict.ContainsKey(key))
            {
                dict[key] = variableValue;
            }
            else dict.Add(key, variableValue);
        }

        if (CoreScriptsManager.OnVariableUpdate != null) CoreScriptsManager.OnVariableUpdate.Invoke(variableName);
    }

}
