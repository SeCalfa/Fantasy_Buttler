﻿using CodeBase.Services;
using CodeBase.Services.Locator;
using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.States
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IState> states;
        private IState activeState;

        public GameStateMachine(SceneLoader sceneLoader)
        {
            GameFactory gameFactory = new GameFactory();
            RandomService randomService = new RandomService();
            GameObjectsLocator gameObjectsLocator = new GameObjectsLocator();

            states = new Dictionary<Type, IState>()
            {
                [typeof(BootstrapState)] = new BootstrapState(this, sceneLoader),
                [typeof(LoadLevelState)] = new LoadLevelState(this, sceneLoader, gameFactory, gameObjectsLocator),
                [typeof(TutorialState)] = new TutorialState(gameObjectsLocator),
                [typeof(PrepearToAttackState)] = new PrepearToAttackState(this, gameFactory, gameObjectsLocator),
                [typeof(AttackingState)] = new AttackingState(randomService, gameObjectsLocator),
                [typeof(PrepearToDefenceState)] = new PrepearToDefenceState(this, gameFactory, gameObjectsLocator),
                [typeof(DefencingState)] = new DefencingState(randomService, gameObjectsLocator),
                [typeof(SkipTurnState)] = new SkipTurnState(gameObjectsLocator),
                [typeof(WinState)] = new WinState(this, gameObjectsLocator),
                [typeof(LoseState)] = new LoseState(this, gameObjectsLocator)
            };
        }

        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }

        public void Enter<TState>(string payload) where TState : class, IState
        {
            TState state = ChangeState<TState>();
            state.EnterWithParam(payload);
        }

        private TState ChangeState<TState>() where TState : class, IState
        {
            activeState?.Exit();

            TState state = GetState<TState>();
            activeState = state;

            return state;
        }

        private TState GetState<TState>() where TState : class, IState =>
            states[typeof(TState)] as TState;
    }
}
