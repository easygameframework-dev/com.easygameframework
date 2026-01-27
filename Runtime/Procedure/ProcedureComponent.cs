//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Fsm;
using EasyGameFramework.Core.Procedure;
using System;
using System.Collections;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 流程组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Procedure")]
    public sealed class ProcedureComponent : GameFrameworkComponent
    {
        private IProcedureManager _procedureManager = null;
        private ProcedureBase _entranceProcedure = null;

        [SerializeField]
        private string[] _availableProcedureTypeNames = null;

        [SerializeField]
        private string _entranceProcedureTypeName = null;

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                return _procedureManager.CurrentProcedure;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                return _procedureManager.CurrentProcedureTime;
            }
        }

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            if (_procedureManager == null)
            {
                Log.Fatal("Procedure manager is invalid.");
                return;
            }
        }

        private IEnumerator Start()
        {
            ProcedureBase[] procedures = new ProcedureBase[_availableProcedureTypeNames.Length];
            for (int i = 0; i < _availableProcedureTypeNames.Length; i++)
            {
                Type procedureType = Utility.Assembly.GetType(_availableProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", _availableProcedureTypeNames[i]);
                    yield break;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", _availableProcedureTypeNames[i]);
                    yield break;
                }

                if (_entranceProcedureTypeName == _availableProcedureTypeNames[i])
                {
                    _entranceProcedure = procedures[i];
                }
            }

            if (_entranceProcedure == null)
            {
                Log.Error("Entrance procedure is invalid.");
                yield break;
            }

            _procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            IsInitialized = true;

            yield return new WaitForEndOfFrame();

            _procedureManager.StartProcedure(_entranceProcedure.GetType());
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            return _procedureManager.HasProcedure<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure(Type procedureType)
        {
            return _procedureManager.HasProcedure(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            return _procedureManager.GetProcedure<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            return _procedureManager.GetProcedure(procedureType);
        }
    }
}
