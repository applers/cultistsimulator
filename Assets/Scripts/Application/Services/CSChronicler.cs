﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Services;
using SecretHistories.Spheres;
using UnityEngine;
using UnityEngine.Analytics;
using Random = System.Random;


namespace SecretHistories.Services
{


    /// <summary>
    /// meta responses to significant in-game events. NB in-game: this is called, used and instantiated on the tabletop.
    /// </summary>
    
    
    public class CSChronicler:AbstractChronicler
    {

        private const string BOOK_ASPECT = "text";
        private const string DESIRE_ASPECT = "desire";
        private const string TOOL_ASPECT = "tool";
        private const string CULT_ASPECT = "society";
        private const string HQ_ASPECT = "hq";
        private const string POWER_ASPECT="powermarks";
        private const string SENSATION_ASPECT="sensationmarks";
        private const string ENLIGHTENMENT_ASPECT = "enlightenmentmarks";
        private const string EXALTED_ASPECT = "exalted";
        private const string DISCIPLE_ASPECT = "disciple";
        private const string FOLLOWER_ASPECT = "follower";
        private const string MORTAL_ASPECT = "mortal";
        private const string SUMMONED_ASPECT = "summoned";
        private const string HIRELING_ASPECT = "hireling";
        private const string EDGE = "edge";
        private const string FORGE = "forge";
        private const string GRAIL = "grail";
        private const string HEART = "heart";
        private const string KNOCK = "knock";
        private const string LANTERN = "lantern";
        private const string MOTH = "moth";
        private const string SECRETHISTORIES = "secrethistories";
        private const string WINTER = "winter";







        public override void TokenPlacedInWorld(Token token)
        {

            if (token.PlacementAlreadyChronicled)
                return;

            if(token.Payload.IsValidElementStack())
            {
                AspectsDictionary tokenAspects = token.Payload.GetAspects(true);

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();

            TryChronicleBookPlaced(token, tokenAspects);

            TryChronicleDesirePlaced(token, tokenAspects);

            TryChronicleFollowerPlaced(token, tokenAspects, storefrontServicesProvider);

            TryChronicleToolPlaced(token, tokenAspects);
            
            TryChronicleCultPlaced(token, tokenAspects, storefrontServicesProvider);
            
            TryCHronicleHQPlaced(token, tokenAspects);

            token.PlacementAlreadyChronicled = true;
            }


        }


        public override void SetAchievementsForEnding(Ending ending)
        {
            if (string.IsNullOrEmpty(ending.Achievement))
                return;

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();
            storefrontServicesProvider.SetAchievementForCurrentStorefronts(ending.Achievement, true);

            if (ending.Achievement == NoonConstants.A_ENDING_MAJORFORGEVICTORY ||
				ending.Achievement == NoonConstants.A_ENDING_MAJORGRAILVICTORY ||
				ending.Achievement == NoonConstants.A_ENDING_MAJORLANTERNVICTORY)
			{
				storefrontServicesProvider.SetAchievementForCurrentStorefronts(NoonConstants.A_ENDING_MAJORVICTORYGENERIC, true);
			}
		}



        public override void ChronicleSpecificsForElementStacksAtGameEnd(List<Token> elementTokens)
        {
            //update best follower
            Element currentFollower=null;

            foreach (var stack in elementTokens.Where(s=>s.GetAspects().ContainsKey(FOLLOWER_ASPECT) && !s.GetAspects().ContainsKey(HIRELING_ASPECT) && !s.GetAspects().ContainsKey(SUMMONED_ASPECT)))
            {
                var aspects = stack.GetAspects();
                //if the follower is Exalted, update it.
                if (aspects.ContainsKey(EXALTED_ASPECT))
                {
                    currentFollower = _compendium.GetEntityById<Element>(stack.Payload.EntityId);

                }

                else if (aspects.ContainsKey(DISCIPLE_ASPECT) && currentFollower!=null && !currentFollower.Aspects.ContainsKey(EXALTED_ASPECT))
                {
                    currentFollower = _compendium.GetEntityById<Element>(stack.Payload.EntityId);
                }
                else if (currentFollower==null || (!currentFollower.Aspects.ContainsKey(EXALTED_ASPECT) &&
                         !currentFollower.Aspects.ContainsKey(DISCIPLE_ASPECT)))
                {
                    currentFollower = _compendium.GetEntityById<Element>(stack.Payload.EntityId);

                }

            }

            if(currentFollower!=null)

                _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastfollower.ToString(), currentFollower.Id);

        }

        private void TryChronicleFollowerPlaced(Token token, AspectsDictionary tokenAspects, StorefrontServicesProvider storefrontServicesProvider)
        {
            if (tokenAspects.ContainsKey(SUMMONED_ASPECT))
			{
				Analytics.CustomEvent( "A_SUMMON_GENERIC", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_SUMMON_GENERIC", true);
			}

            if (tokenAspects.ContainsKey(EXALTED_ASPECT))
            {
                const int EXALT_MINIMUM_ASPECT_LEVEL = 10;

                if (tokenAspects.Keys.Contains(EDGE) && tokenAspects[EDGE]>= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_EDGE" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_EDGE", true);
				}
                if (tokenAspects.Keys.Contains(FORGE) && tokenAspects[FORGE] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_FORGE" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_FORGE", true);
				}
                if (tokenAspects.Keys.Contains(GRAIL) && tokenAspects[GRAIL] >= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_GRAIL" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_GRAIL", true);
				}
                if (tokenAspects.Keys.Contains(HEART) && tokenAspects[HEART] >= EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_HEART" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_HEART", true);
				}
                if (tokenAspects.Keys.Contains(KNOCK) && tokenAspects[KNOCK] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_KNOCK" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_KNOCK", true);
				}
                if (tokenAspects.Keys.Contains(LANTERN) && tokenAspects[LANTERN] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_LANTERN" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_LANTERN", true);
				}
                if (tokenAspects.Keys.Contains(MOTH) && tokenAspects[MOTH] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_MOTH" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_MOTH", true);
				}
                if (tokenAspects.Keys.Contains(WINTER) && tokenAspects[WINTER] >=EXALT_MINIMUM_ASPECT_LEVEL)
				{
					Analytics.CustomEvent( "A_PROMOTED_EXALTED_WINTER" );
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_PROMOTED_EXALTED_WINTER", true);
				}
            }

        }


        private void TryCHronicleHQPlaced(Token token, AspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(HQ_ASPECT))
			{
				Analytics.CustomEvent( "A_HQ_PLACED", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );
                _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastheadquarters.ToString(), token.Payload.EntityId);
			}
        }

        private void TryChronicleCultPlaced(Token token, AspectsDictionary tokenAspects, StorefrontServicesProvider storefrontServicesProvider)
        {
            if (tokenAspects.Keys.Contains(CULT_ASPECT))
            {
				Analytics.CustomEvent( "A_CULT_PLACED", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );
                _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastcult.ToString(), token.Payload.EntityId);

                if (tokenAspects.Keys.Contains("cultsecrethistories_1"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_SECRETHISTORIES", true);
				}
                else if (tokenAspects.Keys.Contains("venerationedge"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_EDGE",true);
				}
                else if (tokenAspects.Keys.Contains("venerationforge"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_FORGE", true);
				}
                else if (tokenAspects.Keys.Contains("venerationgrail"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_GRAIL", true);
				}
                else if (tokenAspects.Keys.Contains("venerationheart"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_HEART", true);
				}
                else if (tokenAspects.Keys.Contains("venerationknock"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_KNOCK", true);
				}
                else if (tokenAspects.Keys.Contains("venerationlantern"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_LANTERN", true);
				}
                else if (tokenAspects.Keys.Contains("venerationmoth"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_MOTH", true);
                }
                else if (tokenAspects.Keys.Contains("venerationwinter"))
				{
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_CULT_WINTER", true);
				}
            }
        }

        private void TryChronicleToolPlaced(Token token, AspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(TOOL_ASPECT))
			{
				Analytics.CustomEvent( "A_TOOL_PLACED", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );
                _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lasttool.ToString(), token.Payload.EntityId);
			}
        }

        private void TryChronicleDesirePlaced(Token token, AspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(DESIRE_ASPECT))
            {
				Analytics.CustomEvent( "A_DESIRE_PLACED", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );

                if (tokenAspects.Keys.Contains(POWER_ASPECT))
				{
					Analytics.CustomEvent( "A_DESIRE_POWER" );
                    _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionpowera");
				}
                else if (tokenAspects.Keys.Contains(SENSATION_ASPECT))
                {
					Analytics.CustomEvent( "A_DESIRE_SENSATION" );
				    _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionsensationa");
				}
                else if (tokenAspects.Keys.Contains(ENLIGHTENMENT_ASPECT))
				{
					Analytics.CustomEvent( "A_DESIRE_ENLIGHTENMENT" );
                    _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastdesire.ToString(), "ascensionenlightenmenta");
				}
            }
        }

        private void TryChronicleBookPlaced(Token token, AspectsDictionary tokenAspects)
        {
            if (tokenAspects.Keys.Contains(BOOK_ASPECT))
			{
				Analytics.CustomEvent( "A_BOOK_PLACED", new Dictionary<string,object>{ {"id",token.Payload.EntityId } } );
                _chroniclingCharacter.SetFutureLegacyEventRecord(LegacyEventRecordId.lastbook.ToString(), token.Payload.EntityId);
			}
        }

        public override void ChronicleOtherworldEntry(string portalEffect)
        {

            var storefrontServicesProvider = Watchman.Get<StorefrontServicesProvider>();

            if(portalEffect.ToLower()==PortalEffect.Wood.ToString().ToLower())
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_WOOD",true);
            else if (portalEffect.ToLower() == PortalEffect.WhiteDoor.ToString().ToLower())
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_WHITEDOOR", true);
            else if (portalEffect.ToLower() == PortalEffect.StagDoor.ToString().ToLower())
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_STAGDOOR", true);
            else if (portalEffect.ToLower() == PortalEffect.SpiderDoor.ToString().ToLower())
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_SPIDERDOOR", true);
            else if (portalEffect.ToLower() == PortalEffect.PeacockDoor.ToString().ToLower())
                storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_PEACOCKDOOR", true);
            else if (portalEffect.ToLower() == PortalEffect.TricuspidGate.ToString().ToLower())
                    storefrontServicesProvider.SetAchievementForCurrentStorefronts("A_MANSUS_TRICUSPIDGATE", true);
                    
        }

    }
}
