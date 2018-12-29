using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using TimeControl;

public class StateDefinitionTest
{
    public class DummyClass
    {
        // recorded, but using the parameter name passing function
        public int i { get; set; }
        // same for this float
        public float f { get; set; }
        [Recorded]
        public double d { get; set; }
        [Recorded]
        public long l { get; set; }
        [Recorded]
        public Vector3 v { get; set; }
        [Recorded]
        public Quaternion q { get; set; }

        public int notRecorded { get; set; }
}

    [Test]
    public void StateDefinitionTestSimplePasses()
    {
        DummyClass dc = new DummyClass();
        DummyClass copy = new DummyClass();

        dc.i = 1;
        dc.f = 2f;
        dc.d = 3d;
        dc.l = 4L;
        dc.v = new Vector3(1, 2, 3);
        dc.q = Quaternion.Euler(30f, 45f, 15f);
        dc.notRecorded = 1;

        StateDefinition sd = StateDefinitionFactory.CreateDefinition(typeof(DummyClass), "i", "f");
        
        byte[] bytes = sd.GetState(dc);
        Debug.Log("State of data container: " + System.BitConverter.ToString(bytes));

        sd.SetState(copy, bytes);
        Assert.AreEqual(dc.i, copy.i);
        Assert.AreEqual(dc.f, copy.f);
        Assert.AreEqual(dc.d, copy.d);
        Assert.AreEqual(dc.l, copy.l);
        Assert.AreNotEqual(dc.notRecorded, copy.notRecorded);
    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator StateDefinitionTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
