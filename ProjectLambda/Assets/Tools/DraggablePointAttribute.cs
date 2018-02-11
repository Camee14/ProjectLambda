using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** Tutorial from: https://gist.github.com/RubenPineda/51d0ca3120a89f7fe5080c05905cb80d
 * 
 * **/
public class DraggablePointAttribute : PropertyAttribute
{
    public bool local;

    public DraggablePointAttribute(bool local = false)
    {
        this.local = local;
    }
}
