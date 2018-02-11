using System;
using System.Collections.Generic;

using UnityEngine;

namespace ReGoap.Unity.FSMExample.OtherScripts
{ // craftable items as well primitive resources
    public class Resource : MonoBehaviour, IResource
    {
        public string ResourceName;
        public float Capacity = 1f;
        protected float startingCapacity;

        protected HashSet<int> reservationList;

        protected virtual void Awake()
        {
            startingCapacity = Capacity;
            reservationList = new HashSet<int>();
        }

        public string GetName()
        {
            return ResourceName;
        }

        public virtual Transform GetTransform()
        {
            return transform;
        }

        public virtual float GetCapacity()
        {
            return Capacity;
        }

        public virtual void RemoveResource(float value)
        {
            if (Capacity > value)
                Capacity -= value;
            else
                Capacity = 0.0f;
        }

        public virtual void Reserve(int id)
        {
            reservationList.Add(id);
        }
        public virtual void Unreserve(int id)
        {
            reservationList.Remove(id);
        }
        public virtual int GetReserveCount()
        {
            return reservationList.Count;
        }
    }

    public interface IResource
    {
        string GetName();
        Transform GetTransform();
        float GetCapacity();
        void RemoveResource(float value);
        void Reserve(int id);
        void Unreserve(int id);
        int GetReserveCount();
    }
}