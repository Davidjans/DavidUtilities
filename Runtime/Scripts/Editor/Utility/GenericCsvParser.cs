#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GenericCsvParser
{
    public static MockDataObject ParseDataRow(List<string> columns, out List<string> warnings)
    {
        warnings = new List<string>();
        var dataObject = ScriptableObject.CreateInstance<MockDataObject>();

        if (columns.Count < 5)
        {
            warnings.Add("Row does not have enough columns (expected 5).");
            return null; 
        }

        dataObject.itemName = columns[0];
        dataObject.description = columns[1];
        
        if (Enum.TryParse<MockDataObject.ItemType>(columns[2], true, out var type))
        {
            dataObject.itemType = type;
        }
        else
        {
            warnings.Add($"Could not parse ItemType '{columns[2]}'. Defaulting to 'None'.");
            dataObject.itemType = MockDataObject.ItemType.None;
        }

        if (bool.TryParse(columns[3], out var unique))
        {
            dataObject.isUnique = unique;
        }
        else
        {
            warnings.Add($"Could not parse boolean '{columns[3]}'. Defaulting to 'false'.");
            dataObject.isUnique = false;
        }
        
        if (int.TryParse(columns[4], out var val))
        {
            dataObject.value = val;
        }
        else
        {
            warnings.Add($"Could not parse integer value '{columns[4]}'. Defaulting to '0'.");
            dataObject.value = 0;
        }

        if (float.TryParse(columns[5], out var w))
        {
            dataObject.weight = w;
        }
        else
        {
            warnings.Add($"Could not parse float weight '{columns[5]}'. Defaulting to '0.0'.");
            dataObject.weight = 0f;
        }

        return dataObject;
    }
}
#endif