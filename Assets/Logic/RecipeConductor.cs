﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Interfaces;

namespace Assets.Core
{
    public interface IRecipeConductor
    {
        Recipe GetLoopedRecipe(Recipe recipe);
        /// <summary>
        ///Determines whether the original recipe, an alternative, or something else should actually be run.
        /// Alternative recipes which match requirements on elements possessed and % chance are run in place of the original recipe.
        /// Alternatives which match, but which specify additional are run after the original recipe.
        /// There may be multiple additional alternatives.
        /// However, if an alternative ever does *not* specify additional, it replaces the entire list (although it may have alternatives of its own)
        /// Alternatives are recursive, and may have additionals of their own.
        /// A non-additional alternative always takes precedence over everything earlier; if a recursive alternative has additionals of its own, they'll replace everything earlier in the execution sequence.
        /// </summary>

        /// <returns> this may be the original recipe, or it may be an alternative recipe, it may be any number of recipes possible including the original</returns>
        IList<Recipe> GetActualRecipesToExecute(Recipe recipe);
    }

    public class RecipeConductor : IRecipeConductor
    {
        private ICompendium compendium;
        private IAspectsDictionary aspectsToConsider;
        private IDice dice;

        public RecipeConductor(ICompendium c,IAspectsDictionary a,IDice d)
        {
            compendium = c;
            aspectsToConsider = a;
            dice = d;
        }

        public Recipe GetLoopedRecipe(Recipe recipe)
        {

            if (recipe.Loop != null)
                return compendium.GetRecipeById(recipe.Loop);

            return null;
        }


        public IList<Recipe> GetActualRecipesToExecute(Recipe recipe)
        {
            IList<Recipe> actualRecipesToExecute = new List<Recipe>() { recipe }; ;
            if (recipe.AlternativeRecipes.Count == 0)
                return actualRecipesToExecute;


            foreach (var ar in recipe.AlternativeRecipes)
            {
                int diceResult = dice.Rolld100();
                if (diceResult <= ar.Chance)
                {
                    Recipe candidateRecipe = compendium.GetRecipeById(ar.Id);
                    if(candidateRecipe.RequirementsSatisfiedBy(aspectsToConsider))
                    //if (candidateRecipeRequirementsAreSatisfied(candidateRecipe))
                    {
                        if (ar.Additional)
                            actualRecipesToExecute.Add(candidateRecipe); //add the additional recipe, and keep going
                        else
                        {
                            IList<Recipe> recursiveRange = GetActualRecipesToExecute(candidateRecipe);//check if this recipe has any substitutes in turn, and then

                            return recursiveRange;//this recipe, or its further alternatives, supersede(s) everything else! return it.
                        }
                    }
                }
            }

            return actualRecipesToExecute; //we either found no matching candidates and are returning the original, or we added one or more additional recipes to the list
        }

    }
}
