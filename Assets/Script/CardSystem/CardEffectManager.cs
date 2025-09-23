using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private PlayerController _controller;
    private PlayerModel _model;

    private void Awake()
    {
        _controller = GetComponent<PlayerController>();
        _model = GetComponent<PlayerModel>();
    }

    public void ActivateEffects(CardData card)
    {
        PhotonView pv = GetComponent<PhotonView>();
        if (!pv.IsMine) return;

        switch (card.cardEffectType)
        {
            case CardEffectType.SlipperyFeet:
                StartCoroutine(DoSlipperyFeet());
                break;
            case CardEffectType.RandomSensitivity:
                StartCoroutine(DoRandomSensivity());
                break;
            case CardEffectType.LightingStop:
                StartCoroutine(DoLightingStop());
                break;
            case CardEffectType.HeavyWeight:
                StartCoroutine(DoHeavyWeight());
                break;
            //case CardEffectType.Blindness:
            //    StartCoroutine(DoBlindness());
            //    break;
        }
    }

    private IEnumerator DoSlipperyFeet()
    {
        Debug.Log("[CardEffect] SlipperyFeet activado");
        float originalDrag = GetComponent<Rigidbody>().drag;

        while (true)
        {
            GetComponent<Rigidbody>().drag = 0.1f; // slippery
            yield return new WaitForSeconds(20f);

            GetComponent<Rigidbody>().drag = originalDrag; // normal
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator DoRandomSensivity()
    {
        Debug.Log("[CardEffect] RandomSensitivity activado");
        float origX = _model.MouseSensivityX;
        float origY = _model.MouseSensivityY;

        while (true)
        {
            _model.MouseSensivityX = Random.Range(1f, 10f);
            _model.MouseSensivityY = Random.Range(1f, 10f);
            yield return new WaitForSeconds(20f);

            _model.MouseSensivityX = origX;
            _model.MouseSensivityY = origY;
            yield return new WaitForSeconds(10f);
        }
    }

    private IEnumerator DoLightingStop()
    {
        Debug.Log("[CardEffect] LightingStop activado");
        while (true)
        {
            yield return new WaitForSeconds(12.5f);
            _controller.SetCanMove(false);
            yield return new WaitForSeconds(2.5f);
            _controller.SetCanMove(true);
        }
    }

    private IEnumerator DoHeavyWeight()
    {
        Debug.Log("[CardEffect] HeavyWeight activado");
        float originalSpeed = _model.Speed;

        while (true)
        {
            typeof(PlayerModel).GetField("_speed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_model, originalSpeed / 2f);

            yield return new WaitForSeconds(20f);

            typeof(PlayerModel).GetField("_speed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_model, originalSpeed);

            yield return new WaitForSeconds(10f);
        }
    }

    //private IEnumerator DoBlindness()
    //{
    //    Debug.Log("[CardEffect] Blindness activado");

    //    while (true)
    //    {
            
    //        yield return new WaitForSeconds(10f);
            
    //        yield return new WaitForSeconds(6.5f);
    //    }
    //}

}
