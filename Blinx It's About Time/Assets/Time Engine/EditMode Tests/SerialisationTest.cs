using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using TimeControl;

public class SerialisationTest {

    [Test]
    public void SerialisationTestSimplePasses()
    {
        byte b = 173;
        short s = 2974;
        int i = 184;
        float f = 294.489f;
        double d = 834.348d;
        long l = -2980476483L;
        ulong ul = 38076064390L;
        ushort us = 36000;

        Assert.AreEqual(b, (byte)  Serialiser.Deserialise(Serialiser.GetBytes(b), 0, typeof(byte)));
        Assert.AreEqual(s, (short) Serialiser.Deserialise(Serialiser.GetBytes(s), 0, typeof(short)));
        Assert.AreEqual(i, (int)   Serialiser.Deserialise(Serialiser.GetBytes(i), 0, typeof(int)));
        Assert.AreEqual(l, (long)  Serialiser.Deserialise(Serialiser.GetBytes(l), 0, typeof(long)));
        Assert.AreEqual(f, (float) Serialiser.Deserialise(Serialiser.GetBytes(f), 0, typeof(float)));
        Assert.AreEqual(d, (double)Serialiser.Deserialise(Serialiser.GetBytes(d), 0, typeof(double)));
        Assert.AreEqual(us,(ushort)Serialiser.Deserialise(Serialiser.GetBytes(us),0, typeof(ushort)));
        Assert.AreEqual(ul,(ulong) Serialiser.Deserialise(Serialiser.GetBytes(ul),0, typeof(ulong)));

    }

    // A UnityTest behaves like a coroutine in PlayMode
    // and allows you to yield null to skip a frame in EditMode
    [UnityTest]
    public IEnumerator SerialisationTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // yield to skip a frame
        yield return null;
    }
}
