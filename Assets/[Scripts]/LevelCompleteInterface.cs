using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILevelCompleteInterface
{
    public void OnLevelCompleted(int level);
    public void OnCloseDoors();
}
