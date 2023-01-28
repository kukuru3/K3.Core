
using UnityEngine;

namespace K3.Utility { 
    public abstract class RollingAverage<T> {
        T[] buffer;
        protected T currentSum;

        int currentIndex = -1;
        bool isFirstPass = true;

        public RollingAverage(int bufferSize) {
            buffer = new T[bufferSize];
        }

        public void Add(T value) {
            currentIndex++;
            if (currentIndex >= buffer.Length) {
                currentIndex = 0;
                isFirstPass = false;
            }
            DecrementSum(buffer[currentIndex]);
            buffer[currentIndex] = value;
            IncrementSum(value);
        }

        public int Length => isFirstPass ? (currentIndex + 1) : buffer.Length;

        public T GetAverage() {
            if (Length == 0) return default;
            return DoGetAverage();
        }

        protected abstract void DecrementSum(T value); //=> currentSum -= value;
        protected abstract void IncrementSum(T value); // => currentSum += value;
        protected abstract T DoGetAverage(); // => currentSum / Length;
    }

    public class RollingAverageInt : RollingAverage<int> {
        public RollingAverageInt(int bufferSize) : base(bufferSize) {  }
        protected override void DecrementSum(int value) => currentSum -= value;
        protected override void IncrementSum(int value) => currentSum += value;
        protected override int DoGetAverage() => currentSum / Length;
    }

    public class RollingAverageFloat : RollingAverage<float> {
        public RollingAverageFloat(int bufferSize) : base(bufferSize) { }
        protected override void DecrementSum(float value) => currentSum -= value;
        protected override void IncrementSum(float value) => currentSum += value;
        protected override float DoGetAverage() => currentSum / Length;
    }

    public class RollingAverageVector3 : RollingAverage<UnityEngine.Vector3> {
        public RollingAverageVector3(int bufferSize) : base(bufferSize) { }

        protected override void DecrementSum(Vector3 value) => currentSum -= value;
        protected override void IncrementSum(Vector3 value) => currentSum += value;
        protected override Vector3 DoGetAverage() => currentSum / Length;
    }

    public class RollingAverageVector2 : RollingAverage<Vector2> {
        public RollingAverageVector2(int bufferSize) : base(bufferSize) { }
        protected override void DecrementSum(Vector2 value) => currentSum -= value;
        protected override Vector2 DoGetAverage() => currentSum / Length;
        protected override void IncrementSum(Vector2 value) => currentSum += value;
    }

}