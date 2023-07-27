using UnityEngine;

namespace Fusion.Sample.DedicatedServer {

  public class PlayerController : NetworkBehaviour {
    
    [SerializeField] private float Speed = 20f;

    private NetworkRigidbody _nrb;

    public override void Spawned() {
      _nrb = GetComponent<NetworkRigidbody>();
    }

    public override void FixedUpdateNetwork() {
      Vector3 direction = default;

      if (GetInput(out NetworkInputPrototype input)) {

        if (input.IsDown(NetworkInputPrototype.BUTTON_FORWARD)) {
          direction += transform.forward;
        }

        if (input.IsDown(NetworkInputPrototype.BUTTON_BACKWARD)) {
          direction -= transform.forward;
        }

        if (input.IsDown(NetworkInputPrototype.BUTTON_LEFT)) {
          direction -= transform.right;
        }

        if (input.IsDown(NetworkInputPrototype.BUTTON_RIGHT)) {
          direction += transform.right;
        }

        direction = direction.normalized;
      }

      // Move player using the NetworkRigidbody Component
      if (_nrb && !_nrb.Rigidbody.isKinematic) {
        _nrb.Rigidbody.AddForce(direction * Speed);
      }
    }
  }
}