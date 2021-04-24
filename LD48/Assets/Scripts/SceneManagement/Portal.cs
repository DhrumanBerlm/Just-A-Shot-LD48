﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace Game.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            A, B
        }

        [SerializeField] int sceneToLoad = -1;
        [SerializeField] Transform spawnPoint = null;
        [SerializeField] DestinationIdentifier destination = DestinationIdentifier.A;
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeInTime = 1.5f;
        [SerializeField] float fadeWaitTime = .5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Coeus"))
            {
                StartCoroutine(TransitionToScene());
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator TransitionToScene()
        {
            if (sceneToLoad < 0)
            {
                Debug.LogError("Scene to lead not set (portal variable).");
                yield break;
            }

            Fader fader = FindObjectOfType<Fader>();

            DontDestroyOnLoad(gameObject);

            if (fader == null)
            {
                print("fader not found");
                yield break;
            }

            yield return fader.FadeOut(fadeOutTime);

            SavingWrapper wrapper = FindObjectOfType<SavingWrapper>();
            wrapper.Save();

            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            wrapper.Load();

            Portal otherPortal = GetOtherPortal();
            UpdatePlayer(otherPortal);
            wrapper.Save();

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fader.FadeIn(fadeInTime);

            Destroy(gameObject);
        }

        private void UpdatePlayer(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Players");
            player.transform.position = otherPortal.spawnPoint.position;
        }

        private Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this || portal.destination != destination) continue;

                return portal;
            }
            return null;
        }
    }
}
