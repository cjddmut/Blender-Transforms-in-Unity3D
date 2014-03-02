using UnityEngine;
using System.Collections;

public abstract class ModalEdit
{
    protected static bool inMode = false;

    public abstract void Start();
    public abstract void Update();
    public abstract void Confirm();
    public abstract void Cancel();
}
