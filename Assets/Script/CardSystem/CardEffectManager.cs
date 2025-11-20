using System.Collections;
using Photon.Pun;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private PlayerController _controller;
    private PlayerModel _model;
    private PhotonView _pv;

    
    private float _baseSpeed;
    private float _baseSprintSpeed;
    private float _baseMouseX;
    private float _baseMouseY;
    private bool _baseIsSlippery;

   
    private Coroutine _randomSensRoutine;
    private Coroutine _lightingStopRoutine;
    private Coroutine _heavyWeightRoutine;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _model = GetComponent<PlayerModel>();
        _pv = GetComponent<PhotonView>();

        
        _baseSpeed = _model.Speed;
        _baseSprintSpeed = _model.SprintSpeed;
        _baseMouseX = _model.MouseSensivityX;
        _baseMouseY = _model.MouseSensivityY;
        _baseIsSlippery = _model.IsSlippery;
    }

    public void ActivateEffects(CardData card)
    {
        if (_pv != null && !_pv.IsMine) return;

        switch (card.cardEffectType)
        {
            case CardEffectType.SlipperyFeet:
                StartCoroutine(DoSlipperyFeet());
                break;

            case CardEffectType.RandomSensitivity:
                if (_randomSensRoutine != null)
                    StopCoroutine(_randomSensRoutine);
                _randomSensRoutine = StartCoroutine(DoRandomSensivity());
                break;

            case CardEffectType.LightingStop:
                if (_lightingStopRoutine != null)
                    StopCoroutine(_lightingStopRoutine);
                _lightingStopRoutine = StartCoroutine(DoLightingStop());
                break;

            case CardEffectType.HeavyWeight:
                if (_heavyWeightRoutine != null)
                    StopCoroutine(_heavyWeightRoutine);
                _heavyWeightRoutine = StartCoroutine(DoHeavyWeight());
                break;
        }
    }

    private IEnumerator DoSlipperyFeet()
    {
        
        _model.IsSlippery = true; 
        yield break;
    }

    private IEnumerator DoRandomSensivity()
    {
        

        while (true)
        {
            _model.MouseSensivityX = Random.Range(0.5f, 20f);
            _model.MouseSensivityY = Random.Range(0.5f, 20f);

            yield return new WaitForSeconds(25f);

            _model.MouseSensivityX = _baseMouseX;
            _model.MouseSensivityY = _baseMouseY;

            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator DoLightingStop()
    {
        

        while (true)
        {
            yield return new WaitForSeconds(12.5f);
            _controller.SetCanMove(false);
            yield return new WaitForSeconds(5f);
            _controller.SetCanMove(true);
        }
    }

    private IEnumerator DoHeavyWeight()
    {
        

        while (true)
        {
            _model.SetMovementSpeed(_baseSpeed / 2f, _baseSpeed / 2f);
            yield return new WaitForSeconds(20f);

            _model.SetMovementSpeed(_baseSpeed, _baseSprintSpeed);
            yield return new WaitForSeconds(10f);
        }
    }

  
    public void ResetAllEffects()
    {
        
        if (_randomSensRoutine != null)
        {
            StopCoroutine(_randomSensRoutine);
            _randomSensRoutine = null;
        }

        if (_lightingStopRoutine != null)
        {
            StopCoroutine(_lightingStopRoutine);
            _lightingStopRoutine = null;
        }

        if (_heavyWeightRoutine != null)
        {
            StopCoroutine(_heavyWeightRoutine);
            _heavyWeightRoutine = null;
        }

      
        _model.IsSlippery = _baseIsSlippery;
        _model.SetMovementSpeed(_baseSpeed, _baseSprintSpeed);
        _model.MouseSensivityX = _baseMouseX;
        _model.MouseSensivityY = _baseMouseY;

       
        if (_controller != null)
        {
            _controller.SetCanMove(true);
        }

        
    }
}
