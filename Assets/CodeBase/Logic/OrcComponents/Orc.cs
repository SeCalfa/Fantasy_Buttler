﻿using CodeBase.Infrastructure.States;
using CodeBase.Logic.PlayerComponents;
using CodeBase.Logic.UI;
using CodeBase.Services.Locator;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Logic.OrcComponents
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
    public class Orc : MonoBehaviour
    {
        [SerializeField]
        private HealthPointPresenter hpPresenter;

        private Transform spawnPoint;
        private GameObjectsLocator gameObjectsLocator;
        private GameStateMachine gameStateMachine;

        private NavMeshAgent agent;
        private Animator animator;

        private bool isMove = false;
        private Vector3 endPoint;
        private ArrowDirection orcAttackSide;
        private ArrowDirection playerDefenceSide;

        public int hp { get; private set; } = 3;

        public void Construct(Transform spawnPoint, GameObjectsLocator gameObjectsLocator, GameStateMachine gameStateMachine)
        {
            this.spawnPoint = spawnPoint;
            this.gameObjectsLocator = gameObjectsLocator;
            this.gameStateMachine = gameStateMachine;
        }

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            Movement();
        }

        public void StartMove(Vector3 endPoint)
        {
            this.endPoint = endPoint;
            isMove = true;
        }

        public void SetOrcAttackSide(ArrowDirection side) =>
            orcAttackSide = side;

        public void SetPlayerDeffenceSide(ArrowDirection side) =>
            playerDefenceSide = side;

        public void TakeDamage()
        {
            hp -= 1;

            if (hp == 0)
            {
                animator.ResetTrigger("Respawn");
                animator.SetTrigger("Die");
            }
            else
                animator.SetTrigger("GetHit");

            hpPresenter.UpdateHp(hp);
        }

        public void ReturnToStartPos()
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        public void Respawn()
        {
            hp = 3;
            hpPresenter.UpdateHp(hp);
            animator.SetTrigger("Respawn");
        }

        private void Movement()
        {
            if (isMove)
            {
                agent.isStopped = false;
                agent.SetDestination(endPoint);
                animator.SetBool("IsRun", true);

                if (Vector3.Distance(transform.position, endPoint) < 0.1f)
                {
                    animator.SetTrigger("Attack");
                    agent.isStopped = true;
                    isMove = false;

                    StartCoroutine(Delay());
                }
            }
        }

        private IEnumerator Delay()
        {
            yield return new WaitForSeconds(3f);

            Player player = gameObjectsLocator.GetGameObjectByName(Constance.PlayerName).GetComponent<Player>();

            if (player.hp == 0)
                gameStateMachine.Enter<LoseState>();
            else
                gameStateMachine.Enter<PrepearToAttackState>();
        }

        // For event on attack animation
        private void Attack()
        {
            animator.SetBool("IsRun", false);

            if (orcAttackSide == playerDefenceSide)
                gameObjectsLocator.GetGameObjectByName(Constance.PlayerName).GetComponent<Player>().TakeDamage();
            else
                Debug.Log("Attack repulsed");
        }
    }
}