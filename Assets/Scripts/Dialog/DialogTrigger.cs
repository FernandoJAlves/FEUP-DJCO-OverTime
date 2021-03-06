﻿using System.Collections;
using UnityEngine;
using FMODUnity;

public class DialogTrigger : Dialog {
    public Collider triggerCollider;

    private void OnTriggerEnter(Collider collider) {
        triggerCollider.enabled = false;
        StartDialog();
    }

    override public void ReEnable() {
        base.ReEnable();

        triggerCollider.enabled = true;
    }
}
