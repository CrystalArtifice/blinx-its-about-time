using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TimeControl
{
    public static class StateDefinitionFactory
    {
        
        public static StateDefinition CreateDefinition(Type t, params string[] recordedProperties)
        {
            Type genType = typeof(StateDefinition<>).MakeGenericType( new Type[] { t } );

            return (StateDefinition)genType.GetMethod("CreateDefinition").Invoke(null, new object[] { recordedProperties });
        }

    }
}
