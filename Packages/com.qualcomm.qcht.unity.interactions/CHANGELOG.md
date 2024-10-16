# CHANGELOG

## QCHT Unity Interaction 4.1.7

### Added

- [QCHTI] Added Android manifest <use-feature android:name="qualcomm.software.handtracking"> for builds using QCHTI.
- [QCHTI] URP support for controller and hand robot shaders.
- [QCHTI] URP support for QCHT Simple lit shader.
- [QCHTI] XR Snap UI activator toggling callbacks.
- [QCHTI] Added startSubsystemOnEnable in XRHandTrackingManager to attempt to start HaT when XRHandTrackingManager is enabled.
  (In case of fusion, AutoStart is disabled in Hand Tracking Feature and startSubsystemOnEnable can be enabled to get the right behaviour)

### Changed

- [QCHTI] Support of inner alpha/color in UI rounded shader.
- [QCHTI] Deprecated Left/Right hand functions in favor of functions with XrHandedness as argument instead in XRHandTrackingManager.
- [QCHTI] Internal poke feedback behaviour using new interfaces.

### Fixed

- [QCHTISamples] Prevent drawing when hovering a UI element.
- [QCHTI-QCHTISamples] Switch to controller issue in Editor mode.
- [QCHTI] Ray reticle display behaviour when UI element is over 3D object.
- [QCHTI] Wrong hand grip orientation was given by the hand tracking simulator in Editor. 
- [QCHTI] Wrong behaviour on Snap Pose Provider scale slider in Editor.
- [QCHTI] Wrong when duplicating Snap Pose in Editor.

### Known issues

- [QCHTI] Do not enable XR_EXT_hand_interaction on tethered based devices.
- [QCHTI] Using interaction profiles, an issue with the input system sets the "isTracked" input control to false even if HaT is actually tracked.
  A work around can be done by checking 'isTracked' input action value directly using OpenXRInput.GetActionIsActive(isTrackedInputAction).

## QCHT Unity Interaction 4.1.6

### Added

- [QCHTI] Poke haptic feedback on UI panels.
- [QCHTI] HaT support can be checked using IsHandTrackingSupported in OXR HandTrackingFeature.
- [QCHTI] Fallback calculation of interaction data and gesture data when QCOM gesture V1 or QCOM gesture V2 are not supported. 
- [QCHTISamples] Reticles and pointer indicators has been added for poke and ray interactors in XRRig/ARRig prefabs by default.

### Changed

- [QCHTISamples] XRSnapUIActivator script has been added to activate/deactivate UI snapping.

### Fixed

- [QCHTI] QCHTI raised compilation errors when switching to standalone platform compilation.
- [QCHTI] Fixed XRRayInteractorLineVisual to block raycast event on non valid targets.
- [QCHTI] Global performances improvements avoiding useless allocations.
- [QCHTI] Fixed camera Y offset in joint calculation when XROrigin has moved.
- [QCHTI] Interaction grid threw warnings with Throw On Detach.
- [QCHTI] XRPassthrough utility unreachable code.
- [QCHTI] Apply VFF to new intances if it was activated before hands instantiation.
- [QCHTISamples] Stop hand tracking when quitting HaT samples in Spaces samples case.

### Known issues

- [QCHTI] Do not enable XR_EXT_hand_interaction on tethered based devices.
- [QCHTI] Using interaction profiles, an issue with the input system sets the "isTracked" input control to false even if HaT is actually tracked.
  A work around can be done by checking 'isTracked' input action value directly using OpenXRInput.GetActionIsActive(isTrackedInputAction).

## QCHT Unity Interaction 4.1.5

### Added

- [QCHTI] Support for XR_EXT_hand_interaction for tethered based devices.
- [QCHTI] Added a Grab filter for backward compatibility.

### Changed

- [QCHTI] Update hand model to better fit with hand tracking data.
- [QCHTI] General code organisation improvement.
- [QCHTI] Update QCHTOpenXR plugin to 1.5.
- [QCHTI] Update XROrigin Utility class to support both ARFoundation and XRIT origins.
- [QCHTSamples] Adjust drawing threshold to be more accurate.

### Fixed

- [QCHTI] Snap pose hand scale when using prefabs.
- [QCHTI] Control Box rotation are now computed in local.

### Known issues

- [QCHTI] Do not enable XR_EXT_hand_interaction on tethered based devices.
- [QCHTI] Using interaction profiles, an issue with the input system sets the "isTracked" input control to false even if HaT is actually tracked.
  A work around can be done by checking 'isTracked' input action value directly using OpenXRInput.GetActionIsActive(isTrackedInputAction).

## QCHT Unity Interaction 4.1.4

### Changed

- [QCHTI] Downgrade Unity OpenXR plugin dependency from 1.9.1 to 1.8.2 because of a discovered memory leak in render texture.


## QCHT Unity Interaction 4.1.3

### Added

- [QCHTI] Added hand skin feedbacks.
- [QCHTSamples] Integrated additional robot hand skin with color feedbacks.

### Changed

 - [QCHTI] Increased Hand extensions calculation accuracy (flexions, curls, abductions and oppositions).
 - [QCHTI] Deprecated XRHandSubsystem related functions in XR Hand Tracking Manager.

### Fixed

- [QCHTI] Hand preview issue when duplicating a snap pose or a snap pose provider in edit mode.
- [QCHTI] Fixed ToggleHand API in XR Hand Manager where hand reappeared even if it was manually disabled.
- [QCHTI] Optimized memory allocations in snap pose provider.

### Known issues

- [QCHTI] Hand interaction profiles can't be enabled along Mixed Realty controller interaction profile on viewer + host device type.
- [QCHTI] Using interaction profiles, an issue with the input system sets the "isTracked" input control to false even if HaT is actually tracked. 
          A work around can be done by checking 'isTracked' input action value directly using OpenXRInput.GetActionIsActive(isTrackedInputAction).

## QCHT Unity Interaction 4.1.2

### Changed

- [QCHTI] Raycast calculation and stabilisation using OneEuroFilter.
- [QCHTI] Separated proximal interactor in two, one for pinching and other one for grasping due to different interaction zones and inputs.
- [QCHTSamples] Filtering pinch and grab using interaction layer masks and Update XRHandFilter to filter without using layer masks. 
- [QCHTI] Set snap poses root relative to hand root raw data instead of XR Direct Interactor device pose.

### Fixed

- [QCHTI] Restore gesture ratio retrieving data from QCOM gesture extension.

### Known issues

- [QCHTI] Hand interaction profiles can't be enabled along Mixed Realty controller interaction profile on viewer + host device type.
- [QCHTI] Using interaction profiles, an issue with the input system sets the "isTracked" input control to false even if HaT is actually tracked. 
          A work around can be done by checking 'isTracked' input action value directly using OpenXRInput.GetActionIsActive(isTrackedInputAction).

## QCHT Unity Interaction 4.1.0

### Added

- [QCHTI] Added pinch position, grasp position corresponding to OpenXR specification.
- [QCHTI] Added interaction profiles support, Microsoft Hand Interaction Profile and Hand Interaction profile (OpenXR plugin >= 1.8.0).
- [Core Assets] New sample named Core Assets providing useful prefabs and presets
    - [Core Assets] Added default action input mapping.
    - [Core Assets] Added XR / AR Rig prefab setup.
- [QCHT Samples] XR Ray Interactor Manager that handles Ray Interactor activation depending on filters.

### Changed

- [QCHTI] Update to XRIT 2.4.3.
- [QCHTI] Raycast calculation and stabilisation.
- [QCHTI] Provide update phase in XR Hand subsystem OnHandUpdated.
- [QCHTI] Set snap poses root relative to hand root raw data instead of XR Direct Interactor device pose.

### Fixed

- [QCHTI] Camera offset calculation when using XR Tracking origin floor for snap poses providers.
- [QCHTI] Hand Tracking Feature issue on package importation when current build platform is other than Android.
- [QCHTI] Rotation offset when hand is fading out.
- [QCHT Samples] Disabled Ray Interactor when drawing.
- [QCHT Samples] Drawing position is now pinch position (between index/thumb tips) and drawing trigger action thresholds has been tweaked.
- [QCHT Samples] Hand orientation controls QCHT Box orientation.

### Known issues

- [QCHTI] Hand interaction profiles should not work properly due to runtime version.

## QCHT Unity Interaction 4.0.5

### Added

- Auto start property in Hand Tracking Feature settings.
- Handling events for QCHT Control Box.

### Changed

- Improved snap pose system using snap pose manager and snap pose receivers.
- Auto disable XR Device Simulator when building for VR Android targets to avoid 6-DOF lock.
- QCHT Hand Tracking simulator works now for standalone builds.
- Update to XRIT 2.4.0.

### Fixed

- Resume HaT subsystem when OpenXR session state focuses again only if HaT was running before the app unfocused. 
- Drawing with controllers in qcht samples.

### Known issues

- OpenXR Interaction controller profile not supported yet by HaT

## QCHT Unity Interaction 4.0.0

### Added

- Raw data visualizer.
- API to subsystem data.
- Switch from Hand to Controllers component.
- Snap poses for different hand scales.
- Hat OpenXR Validation feature.
- Pinch thresholds for triggering and releasing interaction.

### Changed

- Removed QCHT3 backward compatibility and remove QCHT3 assets.
- Subsystem refactoring updating hand tracking in before all mono behaviour update and on before render.
- All subsystem data are in origin space.
- Improve developer experience by adding menu utils and presets.

### Fixed

- Fixed starting and stopping hat with OpenXR Fusion feature. 
- Proximity sensor issue by listening session states changes to start and stop hat.
- Several fixes on samples.
- Conflicts with interactors by using XR Interaction groups.
- QCHT Control Box with XRIT events.

### Known issues

- OpenXR Interaction controller profile not supported yet by HaT

## QCHT Unity Interaction 4.0.0-pre.14

### Added

- XR Poke Interactor support via poke position and poke rotation on index tip
- Hands are custom XR Controller named XR Hand Controller allowing to define two select actions and expose handedness of controller
- XR Hand Filter allowing to filter handedness and interaction gesture on XR Interactables   
- Virtual keyboard interactable with poke and distal interactors has been added in samples assets

### Changed

- Grabpoint has been renamed as XR Hand Interactable Snap pose and improved
- Deprecated Proximal Interactable and Porixmal Interactor scripts as XR Direct Interactor is fully supported
- Updated QCHT samples scenes to match Spaces samples scens menu behaviour
- OpenXR native Plugin upgraded to v1.1.1

### Fixed

- Feedback textures on Pink cube in Proximal Sample
- OpenXR plugin no more stop HaT subsystem if an error is received from getHandData 

### Known issues

- OpenXR Interaction controller profile not supported yet by HaT

## QCHT Unity Interaction 4.0.0-pre.12

### Added

- QCHTStatus enum on XRHandTrackingManager. 
- Hand Tracking samples now inform the user if hand tracking doesn't run, before exiting from the hand trackign scene
- Hand tracking subsytem update the App Space to fully support Floor origin
- Changelog is now part of the hand tracking package 