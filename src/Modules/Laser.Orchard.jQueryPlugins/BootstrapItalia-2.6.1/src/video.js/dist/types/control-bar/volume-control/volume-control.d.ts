export default VolumeControl;
/**
 * The component for controlling the volume level
 *
 * @extends Component
 */
declare class VolumeControl extends Component {
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
    throttledHandleMouseMove: Function;
    handleMouseUpHandler_: (e: any) => void;
    /**
     * Create the `Component`'s DOM element
     *
     * @return {Element}
     *         The element that was created.
     */
    createEl(): Element;
    /**
     * Handle `mousedown` or `touchstart` events on the `VolumeControl`.
     *
     * @param {EventTarget~Event} event
     *        `mousedown` or `touchstart` event that triggered this function
     *
     * @listens mousedown
     * @listens touchstart
     */
    handleMouseDown(event: any): void;
    /**
     * Handle `mouseup` or `touchend` events on the `VolumeControl`.
     *
     * @param {EventTarget~Event} event
     *        `mouseup` or `touchend` event that triggered this function.
     *
     * @listens touchend
     * @listens mouseup
     */
    handleMouseUp(event: any): void;
    /**
     * Handle `mousedown` or `touchstart` events on the `VolumeControl`.
     *
     * @param {EventTarget~Event} event
     *        `mousedown` or `touchstart` event that triggered this function
     *
     * @listens mousedown
     * @listens touchstart
     */
    handleMouseMove(event: any): void;
}
import Component from "../../component.js";
//# sourceMappingURL=volume-control.d.ts.map