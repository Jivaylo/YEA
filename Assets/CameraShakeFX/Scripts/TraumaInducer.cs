 using UnityEngine;
using System.Collections;
public class TraumaInducer : MonoBehaviour 
{
    [Tooltip("Seconds to wait before trigerring the explosion particles and the trauma effect")]
    public float Delay = 0;
    [Tooltip("Maximum stress the effect can inflict upon objects Range([0,1])")]
    public float MaximumStress = 0.6f;
    [Tooltip("Maximum distance in which objects are affected by this TraumaInducer")]
    public float Range = 45;


    public void DoTrauma()
    {
        StartCoroutine(DoTraumaRoutine());
    }

    private IEnumerator DoTraumaRoutine()
    {
        yield return new WaitForSeconds(Delay);

        var targets = FindObjectsByType<StressReceiver>(FindObjectsSortMode.None);

        for (int i = 0; i < targets.Length; ++i)
        {
            var receiver = targets[i];
            if (receiver == null) continue;

            float distance = Vector3.Distance(transform.position, receiver.transform.position);
            if (distance > Range) continue;

            float distance01 = Mathf.Clamp01(distance / Range);
            float stress = (1 - Mathf.Pow(distance01, 2)) * MaximumStress;

            receiver.InduceStress(stress);
        }
    }
}