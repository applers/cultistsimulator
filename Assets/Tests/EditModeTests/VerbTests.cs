using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.Application.Commands.SituationCommands;
using NUnit.Framework;
using SecretHistories.Entities;
using SecretHistories.Entities.Verbs;
using SecretHistories.Interfaces;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;
using UnityEngine.TestTools;

using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[TestFixture]
public class CreationCommandTestsRunWithoutErrors
{

    [SetUp]
    public void WithMinimalCompendiumLoad()
    {
        Watchman.ForgetEverything();

        var compendium = new Compendium();
        var cl = new CompendiumLoader("testcontent");
        var importLog = cl.PopulateCompendium(compendium, "en");
        foreach (var m in importLog.GetMessages())
            Debug.Log(m.Description);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<DeckSpec>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Dictum>().Count);
        Assert.AreEqual(5, compendium.GetEntitiesAsList<Element>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Ending>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Legacy>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<Recipe>().Count);
        Assert.AreEqual(27, compendium.GetEntitiesAsList<Setting>().Count);
        Assert.AreEqual(1, compendium.GetEntitiesAsList<BasicVerb>().Count);

        var watchman = new Watchman();
        watchman.Register(compendium);

        var worldSphere = Object.FindObjectOfType<TabletopSphere>();
        Watchman.Get<SphereCatalogue>().RegisterSphere(worldSphere);
        var enRouteSphere = Object.FindObjectOfType<EnRouteSphere>();
        Watchman.Get<SphereCatalogue>().RegisterSphere(worldSphere);

    }

    [Test]
        public void CreateElementStackToken()
        {
            Assert.AreEqual(1,0);
        }

        [Test]
        public void CreateBasicVerbToken()
        {
            var basicVerb = Watchman.Get<Compendium>().GetEntitiesAsList<BasicVerb>().First();
        var location= new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var basicVerbCreationCommand=new TokenCreationCommand(basicVerb,location,null);
        basicVerbCreationCommand.Execute(Watchman.Get<SphereCatalogue>());

        }


        [Test]
    public void CreateDropzoneVerbToken()
    {
        
        var dropzoneVerb = new DropzoneVerb();
        
        var dropzoneLocation = new TokenLocation(Vector3.zero, Watchman.Get<SphereCatalogue>().GetDefaultWorldSphere());
        var dropzoneCreationCommand = new TokenCreationCommand(dropzoneVerb, dropzoneLocation, null);
        dropzoneCreationCommand.Execute(Watchman.Get<SphereCatalogue>());
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator CreateTokenWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
