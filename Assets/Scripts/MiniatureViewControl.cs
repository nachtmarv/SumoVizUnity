﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureViewControl : MonoBehaviour
{
    public enum HandFunction { Move = 0, Rotate };

    private HandFunction functionLeft = HandFunction.Move;
    private HandFunction functionRight = HandFunction.Rotate;
    private HandManager handManager = null;
    public bool IsHeightLocked { get; set; } = true;

    [SerializeField] private Transform leftHandAnchor = null;
    [SerializeField] private Transform rightHandAnchor = null;
    [SerializeField] private Transform miniatureScaleRotate = null;
    [SerializeField] private Transform miniatureMove = null;
    [SerializeField] private Transform miniaturePedestal = null;
    [SerializeField] private bool grabbingLeftHand = false;
    [SerializeField] private bool grabbingRightHand = false;
    [SerializeField] private float grabThreshold = 0.9f;

    [SerializeField] private Vector3 grabPointMove;
    [SerializeField] private Vector3 grabPointRotateGlobal;
    [SerializeField] private Vector3 grabPointScaleA;
    [SerializeField] private Vector3 grabPointScaleB;

    [SerializeField] private Vector3 savedMiniaturePosGlobal;
    [SerializeField] private Vector3 savedMiniaturePos;
    [SerializeField] private Vector3 savedPedestalPos;
    [SerializeField] private Quaternion savedMiniatureRot;
    [SerializeField] private Vector3 savedMiniatureScale;

    [SerializeField] private float rotAngle;
    [SerializeField] private Vector3 rotInitialVec;
    [SerializeField] private Vector3 rotNewVec;

    private void Awake() {
        handManager = FindObjectOfType<HandManager>();
    }

    void Update()
    {
        if (handManager.axisLeftGrip.getState(grabThreshold))
        {
            if (!grabbingLeftHand) // when not already grabbing
            {
                if (grabbingRightHand)
                {
                    grabPointScaleA = leftHandAnchor.localPosition;
                    grabPointScaleB = rightHandAnchor.localPosition;
                }
                if (functionLeft == HandFunction.Move)
                {
                    grabPointMove = leftHandAnchor.localPosition;
                    grabPointRotateGlobal = rightHandAnchor.position;
                }
                else
                {
                    grabPointMove = rightHandAnchor.localPosition;
                    grabPointRotateGlobal = leftHandAnchor.position;
                }
                saveMiniaturePosition();
                grabbingLeftHand = true;
            }
        }
        else
        {
            if (grabbingLeftHand && grabbingRightHand)
            {
                if (functionLeft == HandFunction.Move)
                {
                    grabPointMove = leftHandAnchor.localPosition;
                    grabPointRotateGlobal = rightHandAnchor.position;
                }
                else
                {
                    grabPointMove = rightHandAnchor.localPosition;
                    grabPointRotateGlobal = leftHandAnchor.position;
                }
            }
            grabbingLeftHand = false;
        }

        if (handManager.axisRightGrip.getState(grabThreshold))
        {
            if (!grabbingRightHand) // when not already grabbing
            {
                if (grabbingLeftHand)
                {
                    grabPointScaleA = leftHandAnchor.localPosition;
                    grabPointScaleB = rightHandAnchor.localPosition;
                }
                if (functionRight == HandFunction.Move)
                {
                    grabPointMove = rightHandAnchor.localPosition;
                    grabPointRotateGlobal = leftHandAnchor.position;
                }
                else
                {
                    grabPointMove = leftHandAnchor.localPosition;
                    grabPointRotateGlobal = rightHandAnchor.position;
                }
                saveMiniaturePosition();
                grabbingRightHand = true;
            }
        }
        else
        {
            if (grabbingLeftHand && grabbingRightHand)
            {
                if (functionRight == HandFunction.Move)
                {
                    grabPointMove = rightHandAnchor.localPosition;
                    grabPointRotateGlobal = leftHandAnchor.position;
                }
                else
                {
                    grabPointMove = leftHandAnchor.localPosition;
                    grabPointRotateGlobal = rightHandAnchor.position;
                }
            }
            grabbingRightHand = false;
        }
        HandleInputs();
    }

    private void HandleInputs()
    {
        // Case rotation or move
        if (grabbingLeftHand != grabbingRightHand) {
            HandFunction currentFunction;
            Transform currentHandAnchor;
            if (grabbingLeftHand)
            {
                currentFunction = functionLeft;
                currentHandAnchor = leftHandAnchor;
            }
            else
            {
                currentFunction = functionRight;
                currentHandAnchor = rightHandAnchor;
            }

            if (currentFunction == HandFunction.Move)
            {
                Vector3 diff = (currentHandAnchor.localPosition - grabPointMove);
                float heightDiff = diff.y;
                diff.y = 0;
                diff = diff * (1/miniatureScaleRotate.localScale.x);
                miniatureMove.localPosition = Quaternion.Inverse(miniatureScaleRotate.rotation) * (savedMiniaturePos + diff);

                if (!IsHeightLocked) {
                    Vector3 pedestalPos = savedPedestalPos;
                    pedestalPos.y += heightDiff;
                    miniaturePedestal.localPosition = pedestalPos;
                }
            }
            else
            {
                rotInitialVec = grabPointRotateGlobal - miniatureScaleRotate.position;
                rotNewVec = currentHandAnchor.position - miniatureScaleRotate.position;
                rotInitialVec.y = 0;
                rotNewVec.y = 0;
                rotAngle = Vector3.SignedAngle(rotInitialVec, rotNewVec, Vector3.up);
                miniatureScaleRotate.localRotation = savedMiniatureRot * Quaternion.Euler(Vector3.up * rotAngle);
            }

        }

        // Case zooming
        if (grabbingLeftHand && grabbingRightHand) {
            float initialDistance = Vector3.Distance(grabPointScaleA, grabPointScaleB);
            float newDistance = Vector3.Distance(leftHandAnchor.localPosition, rightHandAnchor.localPosition);
            Vector3 newScale = savedMiniatureScale * (newDistance / initialDistance);
            miniatureScaleRotate.localScale = newScale;
        }
    }

    void saveMiniaturePosition() {
        savedMiniaturePosGlobal = miniatureMove.position;
        savedMiniaturePos = miniatureScaleRotate.localRotation * miniatureMove.localPosition;
        savedPedestalPos = miniaturePedestal.localPosition;
        savedMiniatureRot = miniatureScaleRotate.localRotation;
        savedMiniatureScale = miniatureScaleRotate.localScale;
    }
}