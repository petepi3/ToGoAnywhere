using System;
using UnityEngine;
using UnityEngine.Events;

namespace Petepi.TGA.Grid
{
    /// <summary>
    /// Base class for all terrain generators.
    /// To work generators must either be attached to the same gameobject as GridSystem
    /// or to a gameobject referenced in a BiomeGenerator.
    /// <seealso cref="GridSystem"/>
    /// <seealso cref="Generators.BiomeGenerator"/>
    /// </summary>
    public abstract class Generator : MonoBehaviour
    {
        /// <summary>
        /// Generate terrain features for a single tile
        /// </summary>
        /// <param name="tile">generated tile</param>
        public abstract void Generate(ref Tile tile);
        
        /// <summary>
        /// Called after seeding random number generators before generation,
        /// cache all parameters that rely on random numbers here
        /// </summary>
        public virtual void OnSeed() {}
        
        /// <summary>
        /// Called before seeding random number generators, once per generation.
        /// Use to initialize generators and reset state from previous generation.
        /// </summary>
        public virtual void OnInit() {}

        // broadcast that values changed in inspector and terrain preview should be regenerated.
        [HideInInspector] public UnityEvent editorPropertyChange = new UnityEvent() ;
        protected virtual void OnValidate()
        {
            editorPropertyChange?.Invoke();
        }
    }
}