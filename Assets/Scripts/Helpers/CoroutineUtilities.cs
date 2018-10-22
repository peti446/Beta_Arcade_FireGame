using System;
using System.Collections;
using UnityEngine;

public static class CoroutineUtilities
{


    ///<summary>
    ///Coroutine function wrapper for a function to be executed after a delay
    ///</summary>
    ///<param name="action">
    ///The action in question, can be lambda, delegates, a functioncall itself...
    ///</param>
    ///<param name="delay">
    ///Time delay to execute the function in seconds
    ///</param>
    ///<example>
    ///StartCoroutine(CoroutineUtils.DelaySeconds(() => DebugUtils.Log("executed after 2 seconds"), 2);
    ///</example>
    public static IEnumerator DelaySeconds(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action();
    }

    ///<summary>
    ///Coroutine function wrapper that will wait for a couple of seconds
    ///</summary>
    ///<param name="time">
    ///Time delay 
    ///</param>
    ///<example>
    ///StartCoroutine(CoroutineUtils.WaitForSeconds(5));
    ///</example>
    public static IEnumerator WaitForSeconds(float time)
    {
        yield return new WaitForSeconds(time);
    }


    ///<summary>
    ///Coroutine function wrapper for a function to be executed in a corroutine
    ///</summary>
    ///<param name="action">
    ///The action in question, can be lambda, delegates, a functioncall itself...
    ///</param>
    ///<example>
    ///StartCoroutine(CoroutineUtils.DelaySeconds(() => Debug.Log("executed in a corroutine"));
    ///</example>
    public static IEnumerator Do(Action action)
    {
        action();
        yield return 0;
    }


    ///<summary>
    ///Coroutine function wrapper for a function to be executed on the next frame
    ///</summary>
    ///<param name="action">
    ///The action in question, can be lambda, delegates, a functioncall itself...
    ///</param>
    ///<example>
    ///StartCoroutine(CoroutineUtils.DoOnNextFrame(() => Debug.Log("executed in a corroutine in the next frame"));
    ///</example>
    public static IEnumerator DoOnNextFrame(Action action)
    {
        yield return 0;
        action();
        yield return 0;
    }
}
