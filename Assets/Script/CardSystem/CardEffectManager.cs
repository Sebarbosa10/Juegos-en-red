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

    
    private Coroutine _activeEffectRoutine;

    
    private CardEffectType? _currentEffect = null;

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
        if (card == null) return;

        Debug.Log($"[CardEffectManager] Activando efecto: {card.cardEffectType}");

        ResetAllEffects();

        _currentEffect = card.cardEffectType;

        switch (card.cardEffectType)
        {
            case CardEffectType.SlipperyFeet:
                _activeEffectRoutine = StartCoroutine(DoSlipperyFeet());
                break;

            case CardEffectType.RandomSensitivity:
                _activeEffectRoutine = StartCoroutine(DoRandomSensitivity());
                break;

            case CardEffectType.LightingStop:
                _activeEffectRoutine = StartCoroutine(DoLightingStop());
                break;

            case CardEffectType.HeavyWeight:
                _activeEffectRoutine = StartCoroutine(DoHeavyWeight());
                break;
        }
    }

    private IEnumerator DoSlipperyFeet()
    {
        Debug.Log("[CardEffectManager] SlipperyFeet activado");
        _model.IsSlippery = true;

       
        ShowEffectUI();

        yield break;
    }

    private IEnumerator DoRandomSensitivity()
    {
        Debug.Log("[CardEffectManager] RandomSensitivity activado");

        while (true)
        {
            
            _model.MouseSensivityX = Random.Range(0.5f, 20f);
            _model.MouseSensivityY = Random.Range(0.5f, 20f);
            ShowEffectUI(); 

            yield return new WaitForSeconds(25f);

            
            _model.MouseSensivityX = _baseMouseX;
            _model.MouseSensivityY = _baseMouseY;
            HideEffectUI(); 

            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator DoLightingStop()
    {
        Debug.Log("[CardEffectManager] LightingStop activado");

        while (true)
        {
            
            HideEffectUI();
            yield return new WaitForSeconds(12.5f);

            
            _controller.SetCanMove(false);
            ShowEffectUI();

            yield return new WaitForSeconds(5f);

            
            _controller.SetCanMove(true);
        }
    }

    private IEnumerator DoHeavyWeight()
    {
        Debug.Log("[CardEffectManager] HeavyWeight activado");

        while (true)
        {
            
            _model.SetMovementSpeed(_baseSpeed / 2f, _baseSprintSpeed / 2f);
            ShowEffectUI();

            yield return new WaitForSeconds(20f);

            
            _model.SetMovementSpeed(_baseSpeed, _baseSprintSpeed);
            HideEffectUI();

            yield return new WaitForSeconds(10f);
        }
    }

    private void ShowEffectUI()
    {
        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.ShowEffectActive();
        }
    }

    private void HideEffectUI()
    {
        if (CardEffectUI.Instance != null)
        {
            CardEffectUI.Instance.HideEffectActive();
        }
    }

    public void ResetAllEffects()
    {
        Debug.Log("[CardEffectManager] Reseteando todos los efectos...");

        if (_activeEffectRoutine != null)
        {
            StopCoroutine(_activeEffectRoutine);
            _activeEffectRoutine = null;
        }

        _model.IsSlippery = _baseIsSlippery;
        _model.SetMovementSpeed(_baseSpeed, _baseSprintSpeed);
        _model.MouseSensivityX = _baseMouseX;
        _model.MouseSensivityY = _baseMouseY;

        if (_controller != null)
        {
            _controller.SetCanMove(true);
        }

        
        HideEffectUI();

        _currentEffect = null;
    }

    public CardEffectType? GetCurrentEffect()
    {
        return _currentEffect;
    }

    public bool HasActiveEffect()
    {
        return _currentEffect.HasValue;
    }

    private void OnDestroy()
    {
        if (_activeEffectRoutine != null)
        {
            StopCoroutine(_activeEffectRoutine);
        }
    }
}