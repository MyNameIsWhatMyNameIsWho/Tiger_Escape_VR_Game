using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class used to register targets being hit by throwables
/// </summary>
public class Target : MonoBehaviour
{
    private Animator _targetHitAnimator;
    private TargetGame _game;
    private bool _wasHit = false;
    private bool _initialized = false;
    void Start()
    {
        _targetHitAnimator = gameObject.GetComponent<Animator>();
    }
    
    public void SetTargetGame(TargetGame game)
    {
        _game = game;
        _initialized = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_wasHit && collision.gameObject.CompareTag("Interactable"))
        {
            _targetHitAnimator.SetBool("Hit", true);
            _wasHit = true;
            _game.TargetHit();
        }
    }
    /// <summary>
    /// When the timer runs out return the targets back up
    /// </summary>
    public void TryEnded()
    {
        _targetHitAnimator.SetBool("Hit", false);
        _wasHit = false;
        _targetHitAnimator.SetBool("TryEnded", true);
    }
    /// <summary>
    /// When the try is started make sure that all animator variables are set up correctly
    /// </summary>
    public void TryStarted() {
        _targetHitAnimator.SetBool("Hit", false);
        _wasHit = false;
        _targetHitAnimator.SetBool("TryEnded", false);
    }
}
