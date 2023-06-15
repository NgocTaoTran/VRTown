using Cysharp.Threading.Tasks;
using Proyecto26;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;
using VRTown.Model;
using Zenject;

namespace VRTown.Service
{
    public partial interface IApiManager
    {
        UniTask<(TResponse, Exception)> GetNonce<TResponse>(string address)
            where TResponse : ApiResponse.IResponse;

        UniTask<Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>>> GetCharacterConfigs();
        UniTask<(AgoraData, System.Exception)> GetAgoraConfiguration();
        UniTask<(string, System.Exception)> LoadPropDatas();
        UniTask<(Sprite, System.Exception)> LoadSpriteAsync(string spritePath);
    }

    public abstract partial class BaseApiManager : IApiManager
    {

        #region Zenject_Binding
        [Inject] protected VRTown.Service.ILogger _logger;
        #endregion Zenject_Binding

        public abstract UniTask<(TResponse, Exception)> GetNonce<TResponse>(string address)
            where TResponse : ApiResponse.IResponse;

        public abstract UniTask<Dictionary<string, Dictionary<string, System.Collections.Generic.Dictionary<string, CharacterItem[]>>>> GetCharacterConfigs();
        public abstract UniTask<(AgoraData, System.Exception)> GetAgoraConfiguration();
        public abstract UniTask<(string, System.Exception)> LoadPropDatas();
        public abstract UniTask<(Sprite, System.Exception)> LoadSpriteAsync(string spritePath);
    }

    public static class PromiseUtility
    {
        public class PromiseAwaiter<TResponse> where TResponse : ApiResponse.IResponse
        {
            private readonly RSG.IPromise<TResponse> _promise;
            private readonly PromiseState[] _donePromiseStates;

            private TResponse _response;
            private Exception _error;

            public bool IsCompleted => _donePromiseStates.Contains(_promise.CurState);

            public PromiseAwaiter(RSG.IPromise<TResponse> promise)
            {
                _donePromiseStates = new PromiseState[]
                {
                    PromiseState.Rejected,
                    PromiseState.Resolved
                };
                _promise = promise;

                _promise
                    .Then(response =>
                    {
                        _response = response;
                        Debug.Log("curState: " + _promise.CurState);
                        Debug.Log("response: " + response.ToString());
                    })
                    .Catch(error => _error = error);
            }

            public async UniTask<(TResponse, Exception)> GetResult()
            {
                await UniTask.WaitUntil(() => IsCompleted);
                return (_response, _error);
            }
        }

        // extension method for Lazy<T>
        // required for await support
        public static PromiseAwaiter<TResponse> GetPromiseAwaiter<TResponse>(
            this RSG.IPromise<TResponse> promise
        ) where TResponse : ApiResponse.IResponse
        {
            return new PromiseAwaiter<TResponse>(promise);
        }

        public class PromiseAwaiter
        {
            private readonly RSG.IPromise<ResponseHelper> _promise;
            private readonly PromiseState[] _donePromiseStates;

            private string _response;
            private Exception _error;

            public bool IsCompleted => _donePromiseStates.Contains(_promise.CurState);

            public PromiseAwaiter(RSG.IPromise<ResponseHelper> promise)
            {
                _donePromiseStates = new PromiseState[]
                {
                    PromiseState.Rejected,
                    PromiseState.Resolved
                };
                _promise = promise;

                _promise.Then(response => _response = response.Text).Catch(error => _error = error);
            }

            public async UniTask<(string, Exception)> GetResult()
            {
                await UniTask.WaitUntil(() => IsCompleted);
                return (_response, _error);
            }
        }

        public static PromiseAwaiter GetPromiseAwaiter(this RSG.IPromise<ResponseHelper> promise)
        {
            return new PromiseAwaiter(promise);
        }
    }
}