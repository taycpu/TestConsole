using UnityEngine;

namespace DebugPanel.Options
{
    public partial class DebugOptions
    {
        private float _prototypePlayerSpeed = 5f;
        private float _prototypeSpawnInterval = 1.25f;
        private float _prototypeHazardSpeed = 2.8f;
        private bool _prototypeInvulnerable;

        [Category("Prototype")]
        [DisplayName("Player Speed")]
        [NumberRange(1f, 12f)]
        [Sort(0)]
        public float PrototypePlayerSpeed
        {
            get => _prototypePlayerSpeed;
            set
            {
                _prototypePlayerSpeed = value;
                OnPropertyChanged(nameof(PrototypePlayerSpeed));
            }
        }

        [Category("Prototype")]
        [DisplayName("Spawn Interval")]
        [NumberRange(0.2f, 5f)]
        [Sort(1)]
        public float PrototypeSpawnInterval
        {
            get => _prototypeSpawnInterval;
            set
            {
                _prototypeSpawnInterval = value;
                OnPropertyChanged(nameof(PrototypeSpawnInterval));
            }
        }

        [Category("Prototype")]
        [DisplayName("Hazard Speed")]
        [NumberRange(0.5f, 8f)]
        [Sort(2)]
        public float PrototypeHazardSpeed
        {
            get => _prototypeHazardSpeed;
            set
            {
                _prototypeHazardSpeed = value;
                OnPropertyChanged(nameof(PrototypeHazardSpeed));
            }
        }

        [Category("Prototype")]
        [DisplayName("Invulnerable")]
        [Sort(3)]
        public bool PrototypeInvulnerable
        {
            get => _prototypeInvulnerable;
            set
            {
                _prototypeInvulnerable = value;
                OnPropertyChanged(nameof(PrototypeInvulnerable));
            }
        }

        [Category("Prototype")]
        [DisplayName("Reset Prototype")]
        [Sort(10)]
        public void ResetPrototype()
        {
            SendPrototypeMessage("ResetPrototype");
        }

        [Category("Prototype")]
        [DisplayName("Emit Test Log")]
        [Sort(11)]
        public void EmitPrototypeLog()
        {
            Debug.Log("[Prototype] Test log from Debug Options.");
        }

        [Category("Prototype")]
        [DisplayName("Emit Test Warning")]
        [Sort(12)]
        public void EmitPrototypeWarning()
        {
            Debug.LogWarning("[Prototype] Test warning from Debug Options.");
        }

        [Category("Prototype")]
        [DisplayName("Emit Test Error")]
        [Sort(13)]
        public void EmitPrototypeError()
        {
            Debug.LogError("[Prototype] Test error from Debug Options.");
        }

        [Category("Prototype")]
        [DisplayName("Emit Log Burst")]
        [Sort(14)]
        public void EmitPrototypeLogBurst()
        {
            SendPrototypeMessage("EmitLogBurst");
        }

        private static void SendPrototypeMessage(string methodName)
        {
            var target = GameObject.Find("PrototypeGame");
            if (target == null)
            {
                Debug.LogWarning("[Prototype] PrototypeGame object was not found in the active scene.");
                return;
            }

            target.SendMessage(methodName, SendMessageOptions.DontRequireReceiver);
        }
    }
}
