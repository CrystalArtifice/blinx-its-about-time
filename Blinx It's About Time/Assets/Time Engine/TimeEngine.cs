using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TimeControl
{
    public enum TimeState
    {
        NOMINAL,
        REWIND,
        FAST_FORWARD,
        PAUSE,
        SLOW,
        PLAYBACK
    }
    public delegate void OnTimeStateChanged(TimeState newState);
    
    
    /// <summary>
    /// Records the state of all registered objects every frame.
    /// </summary>
    public class TimeEngine : MonoBehaviour
    {
        private class RecordableMetadata
        {
            public int jumpFrameIdx;
            public TimeState state;
            public readonly int id;

            public RecordableMetadata(int id, TimeState state)
            {
                jumpFrameIdx = 0;
                this.id = id;
                this.state = state;
            }
        }

        /// <summary>
        /// The default value for the sample buffer.
        /// At 50Hz, this is equivalent to one minute of recording data.
        /// </summary>
        const int DEFAULT_MAX_SAMPLES = 3000;
        
        public enum RecordingState { PAUSED, RECORDING, REWINDING }

        /// <summary>
        /// All of the registered event handlers.
        /// </summary>
        Dictionary<object, List<OnTimeStateChanged>> eventHandlers = new Dictionary<object, List<OnTimeStateChanged>>();

        /// <summary>
        /// The definitions for all recorded types.
        /// </summary>
        Dictionary<Type, StateDefinition> definitions = new Dictionary<Type, StateDefinition>();

        /// <summary>
        /// Maps the recorded objects with their current state.
        /// </summary>
        Dictionary<object, RecordableMetadata> registeredObjects = new Dictionary<object, RecordableMetadata>();

        /// <summary>
        /// A list of samples. The last sample is the most recent.
        /// </summary>
        List<byte[][]> samples = new List<byte[][]>();

        /// <summary>
        /// The next free ID to use if another object is registered.
        /// Corresponds to the index of the objects data in a sample.
        /// </summary>
        int nextObjID = 1; // initialised to 1, as 0 is the sample header

        /// <summary>
        /// Returns the index of the last sample added.
        /// </summary>
        public int CurrentSample
        {
            get { return samples.Count - 1; }
        }

        /// <summary>
        /// The current state of recording.
        /// When paused, no samples will be recorded.
        /// When playing, samples from all objects are recorded.
        /// When rewinding, all objects will have their state set to the latest sample, and the sample will be deleted.
        /// </summary>
        public RecordingState recordingState { get; set; }

        /// <summary>
        /// The maximum samples that can be stored in the sample buffer until a
        /// storage write is triggered.
        /// </summary>
        public int MaxSamples { get; private set; }

        /// <summary>
        /// The size, in bytes, of the current sample buffer.
        /// </summary>
        public int MemoryUsage { get; private set; }

        public TimeEngine()
        {
            // Ensure script execution order is set
            

            MaxSamples = DEFAULT_MAX_SAMPLES;

            // Create default definitions for Unity types
            StateDefinition def;
            def = StateDefinitionFactory.CreateDefinition(typeof(Rigidbody), "position", "rotation", "velocity", "angularVelocity");
            definitions.Add(typeof(Rigidbody), def);

            def = StateDefinitionFactory.CreateDefinition(typeof(Transform), "position", "rotation", "localScale");
            definitions.Add(typeof(Transform), def);
            
            def = StateDefinitionFactory.CreateDefinition(typeof(RectTransform), "position", "rotation", "localScale");
            definitions.Add(typeof(RectTransform), def);

            Play();
        }

        /// <summary>
        /// Registers the given object.
        /// Returns true if it was registered, false if it was already registered.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool Register(object o)
        {
            if (o == null)
                throw new ArgumentException("Object given is null.");

            if(!registeredObjects.ContainsKey(o))
            {
                // Create a definition for this type, if it doesn't exist.
                if (!definitions.ContainsKey(o.GetType()))
                {
                    StateDefinition sd = StateDefinitionFactory.CreateDefinition(o.GetType());
                    definitions.Add(o.GetType(), sd);
                }

                registeredObjects.Add(o, new RecordableMetadata(nextObjID, TimeState.NOMINAL));
                nextObjID++;
                //print("Registered object " + o.ToString() + ". Hashcode: " + o.GetHashCode());
                return true;
            }else
            {
                print("Dictionary contains key " + o + ": " + registeredObjects.ContainsKey(o));
            }
            return false;
        }
        
        /// <summary>
        /// Deregisters the given object.
        /// Returns true if it was deregistered, false if it was not registered.
        /// </summary>
        /// <returns></returns>
        public bool Deregister(object o)
        {
            return registeredObjects.Remove(o);
        }

        /// <summary>
        /// Adds a definition for a type, defining which properties will be recorded.
        /// </summary>
        /// <param name="s"></param>
        public void AddDefinition(StateDefinition s)
        {
            if (!definitions.ContainsKey(s.Type))
            {
                definitions.Add(s.Type, s);
            }else
            {
                throw new InvalidOperationException("Cannot add new definition for type '" + s.Type + "', as it already exists.");
            }
        }

        /// <summary>
        /// Subscribes an event handler when the time flow state changes on the given object, o.
        /// </summary>
        /// <param name="eventHandler"></param>
        /// <param name="o"></param>
        public void Subscribe(OnTimeStateChanged eventHandler, object o)
        {
            if(!registeredObjects.ContainsKey(o))
                return;

            if (!eventHandlers.ContainsKey(o))
            {
                var newList = new List<OnTimeStateChanged>();
                newList.Add(eventHandler);
                eventHandlers.Add(o, newList);
                //Debug.Log("Added " + o + " to event handlers. Hash: " + o.GetHashCode());
            }else
            {
                List<OnTimeStateChanged> list;
                eventHandlers.TryGetValue(o, out list);
                list.Add(eventHandler);
                //Debug.Log("Added " + eventHandler + " to event handlers for " + o + ". Hash: " + o.GetHashCode());
            }
        }

        public void Unsubscribe(OnTimeStateChanged eventHandler, object o)
        {
            Debug.Log("Unsubscribed " + o);
            List<OnTimeStateChanged> l;

            if(eventHandlers.TryGetValue(o, out l))
            {
                l.Remove(eventHandler);
            }
        }

        public void FixedUpdate()
        {
            // Pause: simply early out
            // Play: set the state of any objects that need it, record the current state and store it
            // Rewind: set the state of ALL objects to the latest sample before discarding it
            if (recordingState != RecordingState.PAUSED)
            {
                if (recordingState == RecordingState.RECORDING)
                {
                    FixedPlayUpdate();
                } else if (recordingState == RecordingState.REWINDING)
                {
                    FixedRewindUpdate();
                }
            }
        }

        /// <summary>
        /// Stops recording. Rewinds all objects until Play() is called or the sample list is exhausted.
        /// Note that this affects all objects globally, and any state stored during
        /// the rewind will be wiped.
        /// </summary>
        public void Rewind()
        {
            recordingState = RecordingState.REWINDING;
        }

        /// <summary>
        /// Stops recording, if it is.
        /// </summary>
        public void Pause()
        {
            recordingState = RecordingState.PAUSED;
        }

        /// <summary>
        /// Resumes recording, if it wasn't already.
        /// </summary>
        public void Play()
        {
            recordingState = RecordingState.RECORDING;
        }

        private void FixedRewindUpdate()
        {
            // If there are no more samples, stop the rewind.
            // TODO: Read in any previously recorded state from the hard disk, if streaming to storage is implemented.
            if (samples.Count == 0)
                return;

            RecordableMetadata meta;
            int sampleIdx = samples.Count - 1;

            // Set the state of all objects before the sample is discarded
            foreach (object o in registeredObjects.Keys)
            {
                registeredObjects.TryGetValue(o, out meta);

                SetState(o, sampleIdx);
            }

            // Remove the current sample from the list
            // If there is only one sample left, leave it so the state is set repeatedly.
            if (samples.Count > 1)
            {
                int sampleSize;
                sampleSize = BitConverter.ToInt32(samples[CurrentSample][0], 4);
                MemoryUsage -= sampleSize;
                samples.RemoveAt(CurrentSample);
            }
        }

        /// <summary>
        /// Records the current state of all objects, modifying any with different time states.
        /// </summary>
        private void FixedPlayUpdate()
        {
            RecordableMetadata meta;
            byte[][] newSample = CreateSample();
            int numBytes = 0;
            foreach (object o in registeredObjects.Keys)
            {
                registeredObjects.TryGetValue(o, out meta);

                UpdateState(o);

                numBytes += RecordState(o, newSample);
            }
            SetSampleByteSize(newSample, numBytes);
            MemoryUsage += numBytes;
            samples.Add(newSample);
        }
        
        /// <summary>
        /// Sets the time flow state for all registered objects.
        /// </summary>
        /// <param name="state"></param>
        public void SetTimeState(TimeState state)
        {
            foreach(object o in registeredObjects.Keys)
            {
                SetTimeState(o, state);
            }
        }

        /// <summary>
        /// Sets the time flow state for all registered objects, excluding any given.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="exclusions"></param>
        public void SetTimeState(TimeState state, params object[] exclusions)
        {
            // Add exclusions to a hashset to reduce the time complexity from O(mn) to O(m+n)
            HashSet<object> exclusionSet = new HashSet<object>(exclusions);

            foreach (object o in registeredObjects.Keys)
            {
                if(!exclusionSet.Contains(o))
                    SetTimeState(o, state);
            }
        }

        /// <summary>
        /// Sets the state of the given object, if it is registered.
        /// </summary>
        /// <param name="state">The time state the object will be set to.</param>
        public void SetTimeState(object obj, TimeState state)
        {
            //Debug.Log("Set time state to " + state + " for object " + obj);
            if (!registeredObjects.ContainsKey(obj))
                throw new ArgumentException("The object " + obj + " is not registered.");
            
            // Get all the behaviour systems for this object and update them,
            // if any exist.
            
            List<OnTimeStateChanged> handlers;
            if(eventHandlers.TryGetValue(obj, out handlers))
            {
                foreach(OnTimeStateChanged handler in handlers)
                {
                    handler(state);
                }
            }else
            {
                Debug.Log("No event handlers for " + obj);
            }

            var meta = GetMetadata(obj);
            meta.state = state;

            switch (state)
            {
                case TimeState.NOMINAL:
                case TimeState.SLOW:
                case TimeState.FAST_FORWARD:
                case TimeState.REWIND:
                    meta.jumpFrameIdx = CurrentSample;
                    break;
                case TimeState.PLAYBACK:
                    break;
            }

        }

        /// <summary>
        /// Creates a new sample, formatting the header.
        /// </summary>
        /// <returns></returns>
        private byte[][] CreateSample()
        {
            byte[][] result = new byte[registeredObjects.Count + 1][];
            // Make the header
            // 0-3: Int32 representing the number of objects in the sample
            byte[] sampleSize = BitConverter.GetBytes(registeredObjects.Count);

            result[0] = new byte[]
            {
                // Size of the sample (number of registered objects)
                sampleSize[0], sampleSize[1], sampleSize[2], sampleSize[3],
                // Size of sample (in total bytes, not counting the header)
                0, 0, 0, 0
            };
            return result;
        }
        
        /// <summary>
        /// Sets the "Byte Size" attribute of the given sample.
        /// This attribute describes the number of bytes stored in the sample.
        /// </summary>
        private void SetSampleByteSize(byte[][] sample, int val)
        {
            byte[] asBytes = BitConverter.GetBytes(val);
            for(int ii = 0; ii < 4; ii++)
            {
                sample[0][4 + ii] = asBytes[ii];
            }
        }

        /// <summary>
        /// Updates the state of the object, if it needs it.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="meta"></param>
        private void UpdateState(object o)
        {
            RecordableMetadata meta = GetMetadata(o);
            
            switch (meta.state)
            {
                case TimeState.NOMINAL:
                case TimeState.SLOW:
                case TimeState.FAST_FORWARD:
                case TimeState.PAUSE:
                    meta.jumpFrameIdx++;
                    break;
                case TimeState.REWIND:
                    SetState(o, meta.jumpFrameIdx);
                    //Debug.Log("Set state of " + o + " to frame " + meta.jumpFrameIdx);
                    meta.jumpFrameIdx--;
                    if(meta.jumpFrameIdx < 0)
                    {
                        meta.jumpFrameIdx = 0;
                    }
                    break;
                case TimeState.PLAYBACK:
                    // Set the state, advance the jump frame
                    SetState(o, meta.jumpFrameIdx);
                    meta.jumpFrameIdx++;
                    // ensure the frame index never goes out of range.
                    // once this happens, the state is set to nominal
                    if (meta.jumpFrameIdx >= samples.Count)
                    {
                        SetTimeState(o, TimeState.NOMINAL);
                    }
                    break;
                default:
                    throw new InvalidProgramException("Invalid time flow state: " + meta.state);
            }

        }

        /// <summary>
        /// Sets the state of an object to the state it had in the given sample index.
        /// If there was no recording data for the given sample, or the sample index is
        /// out of range, the state will not be set.
        /// Returns true if the state was set.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="sampleIdx"></param>
        private bool SetState(object o, int sampleIdx)
        {
            RecordableMetadata meta = GetMetadata(o);
            StateDefinition def = GetDefinition(o.GetType());
            byte[][] sample = samples[sampleIdx];

            if (meta.id < sample.Length)
            {
                def.SetState(o, sample[meta.id]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the state of the object and records it into the given sample.
        /// Returns the number of bytes that were recorded.
        /// </summary>
        /// <param name="o"></param>
        private int RecordState(object o, byte[][] sample)
        {
            RecordableMetadata meta = GetMetadata(o);
            StateDefinition def;
            definitions.TryGetValue(o.GetType(), out def);
            sample[meta.id] = def.GetState(o);
            return sample[meta.id].Length;
        }

        /// <summary>
        /// O(1). Returns the metadata for the given object, o.
        /// Throws ArgumentException if it is not registered.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private RecordableMetadata GetMetadata(object o)
        {
            RecordableMetadata meta;
            if (registeredObjects.TryGetValue(o, out meta))
            {
                return meta;
            }
            throw new ArgumentException("The given object '" + o.ToString() + "' is not registered. Hashcode: " + o.GetHashCode());
        }

        /// <summary>
        /// O(1). Returns the definition for the given type, t.
        /// Throws ArgumentException if it has no definition.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private StateDefinition GetDefinition(Type t)
        {
            StateDefinition def;
            if(definitions.TryGetValue(t, out def))
                return def;

            throw new ArgumentException("The given type '" + t + "' has no definition.");
        }
    }
}