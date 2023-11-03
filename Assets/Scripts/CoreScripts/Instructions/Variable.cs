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

    public static bool ComparisonOfType(string type, string val1, string val2)
    {
        return (ValueStartsWith(val1, type) || ValueStartsWith(val2, type));
    }

    // TODO: Make SqrDistance maybe work with two SqrDistance operations (dynamically checking two distances against each other)
    public static void CompareVals(string val1, string val2, string comp, Condition c, ConditionBlock cb)
    {
        if (string.IsNullOrEmpty(comp)) comp = "Eq";
        var ID = $"{cb.ID}-{c.ID}";
        var sqDistComp = ComparisonOfType("SqrDistance", val1, val2);
        if (sqDistComp)
        {
            var data = new ProximityData();
            var distVal = val1;
            if (val2.StartsWith("SqrDistance"))
            {
                distVal = val2;
                if (comp == "Lt") comp = "Gt";
                else if (comp == "Gt") comp = "Lt";
                data.distanceValue = float.Parse(val1);
            }
            else data.distanceValue = float.Parse(val2);
            if (distVal.StartsWith("SqrDistance"))
            {
                GetEntitiesForSqrDistance(val1, out data.ent1ID, out data.ent2ID, out data.t1, out data.t2);
                data.cond = c;
                data.block = cb;
                data.comp = comp;
                if (RunDistanceCondition(data)) SatisfyCondition(c, cb);
                else CoreScriptsManager.instance.distanceConditions.Add(ID, data);
                return;
            }
        }


        if (!CompareValsHelper(val1, val2, comp, c, cb))
        {
            VariableChangedDelegate del = (x) =>
            {
                if (x == val1 || x == val2 || 
                    ComparisonOfType("MissionStatus", val1, val2))
                {
                    CompareValsHelper(val1, val2, comp, c, cb);
                }

            };

            CoreScriptsManager.instance.variableChangedDelegates.Add(ID, del);
            CoreScriptsManager.OnVariableUpdate += del;
        }
    }


    private static void GetEntitiesForSqrDistance(string sqrDistanceStr, out string ent1ID, out string ent2ID, out Transform t1, out Transform t2)
    {
        Debug.LogWarning(sqrDistanceStr);
        var tmp = sqrDistanceStr.Trim().Replace("SqrDistance", "").Replace(")", "").Trim().Replace("(", "").Trim();
        var entArr = tmp.Split(",");
        if (entArr.Length != 2)
        {
            throw new System.Exception($"Distance comparison requires two entity values, got {entArr.Length}");
        }

        ent1ID = entArr[0].Trim();
        ent2ID = entArr[1].Trim();
        t1 = GetProximityTransformFromID(ent1ID);
        t2 = GetProximityTransformFromID(ent2ID);
    }

    private static string GetComparisonValue(string val)
    {
        val = VariableSensitizeValue(val);
        if (val.StartsWith("MissionStatus"))
        {
            var missionName = val.Trim().Replace("MissionStatus", "").Replace(")", "").Trim().Replace("(", "").Trim();
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
        Debug.Log($"Comparison of type: {comp} with values {tmp1} and {tmp2}");
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

        if (CoreScriptsManager.OnVariableUpdate != null) 
        {
            Debug.Log($"<SetVariable> invoking on {variableName}");
            CoreScriptsManager.OnVariableUpdate.Invoke(variableName);
        }
    }

}
