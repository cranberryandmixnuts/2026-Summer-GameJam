using UnityEngine;

internal sealed class PoolResetState
{
    private readonly PooledObject owner;
    private readonly bool rootWasActive;
    private readonly TransformState[] transforms;
    private readonly ActiveState[] childActiveStates;
    private readonly Rigidbody[] rigidbodies;
    private readonly Rigidbody2D[] rigidbodies2D;
    private readonly TrailRenderer[] trails;
    private readonly ParticleSystem[] particles;
    private readonly AudioSource[] audioSources;

    internal bool RootWasActive => rootWasActive;

    internal PoolResetState(PooledObject owner, bool rootWasActive)
    {
        this.owner = owner;
        this.rootWasActive = rootWasActive;

        Transform[] foundTransforms = owner.GetComponentsInChildren<Transform>(true);
        transforms = new TransformState[foundTransforms.Length];
        for (int i = 0; i < foundTransforms.Length; i++)
            transforms[i] = new TransformState(foundTransforms[i]);

        childActiveStates = new ActiveState[Mathf.Max(0, foundTransforms.Length - 1)];
        for (int i = 1; i < foundTransforms.Length; i++)
            childActiveStates[i - 1] = new ActiveState(foundTransforms[i].gameObject);

        rigidbodies = owner.ResetRigidbodies
            ? owner.GetComponentsInChildren<Rigidbody>(true)
            : System.Array.Empty<Rigidbody>();
        rigidbodies2D = owner.ResetRigidbodies
            ? owner.GetComponentsInChildren<Rigidbody2D>(true)
            : System.Array.Empty<Rigidbody2D>();
        trails = owner.ClearTrailRenderers
            ? owner.GetComponentsInChildren<TrailRenderer>(true)
            : System.Array.Empty<TrailRenderer>();
        particles = owner.ClearParticleSystems
            ? owner.GetComponentsInChildren<ParticleSystem>(true)
            : System.Array.Empty<ParticleSystem>();
        audioSources = owner.StopAudioSources
            ? owner.GetComponentsInChildren<AudioSource>(true)
            : System.Array.Empty<AudioSource>();
    }

    internal void RestoreForSpawn()
    {
        if (owner.RestoreTransformHierarchy)
        {
            for (int i = 0; i < transforms.Length; i++)
                transforms[i].Restore();
        }

        if (owner.RestoreChildActiveStates)
        {
            for (int i = 0; i < childActiveStates.Length; i++)
                childActiveStates[i].Restore();
        }

        ResetTransientState();
    }

    internal void ResetForDespawn()
    {
        ResetTransientState();
    }

    private void ResetTransientState()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Rigidbody body = rigidbodies[i];
            if (body == null || body.isKinematic)
                continue;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        for (int i = 0; i < rigidbodies2D.Length; i++)
        {
            Rigidbody2D body = rigidbodies2D[i];
            if (body == null || body.bodyType == RigidbodyType2D.Static)
                continue;

            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        for (int i = 0; i < trails.Length; i++)
        {
            if (trails[i] != null)
                trails[i].Clear();
        }

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem particle = particles[i];
            if (particle == null)
                continue;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Clear(true);
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
                audioSources[i].Stop();
        }
    }

    private readonly struct TransformState
    {
        private readonly Transform transform;
        private readonly Vector3 localPosition;
        private readonly Quaternion localRotation;
        private readonly Vector3 localScale;

        internal TransformState(Transform transform)
        {
            this.transform = transform;
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }

        internal void Restore()
        {
            if (transform == null)
                return;

            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;
        }
    }

    private readonly struct ActiveState
    {
        private readonly GameObject gameObject;
        private readonly bool activeSelf;

        internal ActiveState(GameObject gameObject)
        {
            this.gameObject = gameObject;
            activeSelf = gameObject.activeSelf;
        }

        internal void Restore()
        {
            if (gameObject != null && gameObject.activeSelf != activeSelf)
                gameObject.SetActive(activeSelf);
        }
    }
}