using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
        return ValueStartsWith(val1, type) || ValueStartsWith(val2, type);
    }

    public static bool ComparisonOfType(string type, string val1, string val2, string comp, out string outVal1, out string outVal2, out string outComp)
    {
        outVal1 = val1;
        outVal2 = val2;
        if (ValueStartsWith(val1, type))
        {
            outComp = comp;
            return true;
        }
        if (ValueStartsWith(val2, type))
        {
            outVal1 = val2;
            outVal2 = val1;
            outComp = comp;
            if (comp == "Lt")
                outComp = "Gt";
            else if (comp == "Gt")
                outComp = "Lt";
            return true;
        }
        outComp = comp;
        return false;
    }

    // TODO: Make SqrDistance maybe work with two SqrDistance operations (dynamically checking two distances against each other)
    public static void CompareVals(string val1, string val2, string comp, Condition c, ConditionBlock cb)
    {
        if (string.IsNullOrEmpty(comp)) comp = "Eq";
        var ID = $"{cb.ID}-{c.ID}";
        var sqDistComp = ComparisonOfType("SqrDistance", val1, val2, comp, out val1, out val2, out comp);
        if (sqDistComp)
        {
            SqDistComp(val1, val2, comp, ID, c, cb);
            return;
        }

        // call this to make sure mission status is arg 1
        ComparisonOfType("MissionStatus", val1, val2, comp, out val1, out val2, out comp);

        // if the var is already done just return
        if (CompareValsHelper(val1, val2, comp, c, cb))
        {
            return;
        }

        AddDelegate(val1, val2, comp, c, cb, ID);
    }

    private static void AddDelegate(string val1, string val2, string comp, Condition c, ConditionBlock cb, string ID)
    {
        VariableChangedDelegate del = (x) =>
        {
            if (x == val1 || x == val2 || val1.StartsWith("MissionStatus"))
            {
                CompareValsHelper(val1, val2, comp, c, cb);
            }

        };

        var varCond = CoreScriptsManager.instance.variableChangedDelegates;
        if (varCond.ContainsKey(ID)) varCond.Remove(ID);
        varCond.Add(ID, del);
        CoreScriptsManager.OnVariableUpdate += del;
    }

    // val1 must be the sqrdistance
    private static void SqDistComp(string val1, string val2, string comp, string ID, Condition c, ConditionBlock cb)
    {
        var data = new ProximityData();
        var distVal = val1;
        data.distanceValue = float.Parse(val2);
        GetEntitiesForSqrDistance(val1, out data.ent1ID, out data.ent2ID, out data.t1, out data.t2);
        data.cond = c;
        data.block = cb;
        data.comp = comp;
        if (RunDistanceCondition(data)) SatisfyCondition(c, cb);
        else
        {
            var dCond = CoreScriptsManager.instance.distanceConditions;
            if (dCond.ContainsKey(ID)) dCond.Remove(ID);
            dCond.Add(ID, data);
        }
    }

    private static void GetEntitiesForSqrDistance(string sqrDistanceStr, out string ent1ID, out string ent2ID, out Transform t1, out Transform t2)
    {
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
        if (val.StartsWith("Shards"))
        {
            return PlayerCore.Instance.cursave.shards.ToString();
        }
        if (val.StartsWith("Gas"))
        {
            return PlayerCore.Instance.cursave.gas.ToString();
        }
        if (val.StartsWith("FusionEnergy"))
        {
            return PlayerCore.Instance.cursave.fusionEnergy.ToString();
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
                var tmpVal1 = float.Parse(tmp1, CultureInfo.InvariantCulture);
                var tmpVal2 = float.Parse(tmp2, CultureInfo.InvariantCulture);
                if (tmpVal1 > tmpVal2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
            case "Lt":
                tmpVal1 = float.Parse(tmp1, CultureInfo.InvariantCulture);
                tmpVal2 = float.Parse(tmp2, CultureInfo.InvariantCulture);
                if (tmpVal1 < tmpVal2)
                {
                    SatisfyCondition(c, cb);
                    return true;
                }
                break;
        }
        return false;
    }

    public static void SetVariable(string variableName, string variableValue, bool onlyIfNull = false)
    {
        var key = "";
        if (variableName.StartsWith("$$$"))
        {
            var names = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames;
            var vals = SaveHandler.instance.GetSave().coreScriptsGlobalVarValues;
            key = variableName.Substring(3).Trim();
            var index = names.IndexOf(key);
            if (index >= 0) 
            {
                if (!onlyIfNull)
                    vals[index] = variableValue;
            }
            else
            {
                names.Add(key);
                vals.Add(variableValue);
            }
        }
        else if (variableName.StartsWith("$$"))
        {
            var dict = CoreScriptsManager.instance.globalVariables;
            key = variableName.Substring(2).Trim();
            if (dict.ContainsKey(key))
            {
                if (!onlyIfNull)
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
