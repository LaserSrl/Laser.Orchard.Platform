export default VolumePanel;
/**
 * A Component to contain the MuteToggle and VolumeControl so that
 * they can work together.
 *
 * @extends Component
 */
declare class VolumePanel extends Component {
    /**
     * Creates an instance of this class.
     *
     * @param {Player} player
     *        The `Player` that this class should be attached to.
     *
     * @param {Object} [options={}]
     *        The key/value store of player options.
     */
    constructor(player: Player, options?: any);
    handleKeyPressHandler_: (e: any) => void;
    /**
     * Add vjs-slider-active class to the VolumePanel
     *
     * @listens VolumeControl#slideractive
     * @private
     */
    private sliderActive_;
    /**
     * Removes vjs-slider-active class to the VolumePanel
     *
     * @listens VolumeControl#sliderinactive
     * @private
     */
    private sliderInactive_;
    /**
     * Adds vjs-hidden or vjs-mute-toggle-only to the VolumePanel
     * depending on MuteToggle and VolumeControl state
     *
     * @listens Player#loadstart
     * @private
     */
    private volumePanelState_;
    /**
     * Create the `Component`'s DOM element
     *
     * @return {Element}
     *         The element that was created.
     */
    createEl(): Element;
    /**
     * Dispose of the `volume-panel` and all child components.
     */
    dispose(): void;
    /**
     * Handles `keyup` events on the `VolumeControl`, looking for ESC, which closes
     * the volume panel and sets focus on `MuteToggle`.
     *
     * @param {EventTarget~Event} event
     *        The `keyup` event that caused this function to be called.
     *
     * @listens keyup
     */
    handleVolumeControlKeyUp(event: any): void;
    /**
     * This gets called when a `VolumePanel` gains hover via a `mouseover` event.
     * Turns on listening for `mouseover` event. When they happen it
     * calls `this.handleMouseOver`.
     *
     * @param {EventTarget~Event} event
     *        The `mouseover` event that caused this function to be called.
     *
     * @listens mouseover
     */
    handleMouseOver(event: any): void;
    /**
     * This gets called when a `VolumePanel` gains hover via a `mouseout` event.
     * Turns on listening for `mouseout` event. When they happen it
     * calls `this.handleMouseOut`.
     *
     * @param {EventTarget~Event} event
     *        The `mouseout` event that caused this function to be called.
     *
     * @listens mouseout
     */
    handleMouseOut(event: any): void;
}
import Component from "../component.js";
//# sourceMappingURL=volume-panel.d.ts.map