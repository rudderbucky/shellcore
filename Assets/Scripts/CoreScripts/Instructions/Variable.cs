using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variable : MonoBehaviour
{
    public static void SetVariable(string variableName, string variableValue)
    {
        if (variableName.StartsWith("$$$"))
        {
            var names = SaveHandler.instance.GetSave().coreScriptsGlobalVarNames;
            var vals = SaveHandler.instance.GetSave().coreScriptsGlobalVarValues;
            var key = variableName.Substring(3);
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
            var key = variableName.Substring(2);
            if (dict.ContainsKey(key))
            {
                dict[key] = variableValue;
            }
            else dict.Add(key, variableValue);
        }
    }

}
