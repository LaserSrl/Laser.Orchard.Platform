export default PlayToggle;
/**
 * Button to toggle between play and pause.
 *
 * @extends Button
 */
declare class PlayToggle extends Button {
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
    /**
     * This gets called when an `PlayToggle` is "clicked". See
     * {@link ClickableComponent} for more detailed information on what a click can be.
     *
     * @param {EventTarget~Event} [event]
     *        The `keydown`, `tap`, or `click` event that caused this function to be
     *        called.
     *
     * @listens tap
     * @listens click
     */
    handleClick(event: any): void;
    /**
     * This gets called once after the video has ended and the user seeks so that
     * we can change the replay button back to a play button.
     *
     * @param {EventTarget~Event} [event]
     *        The event that caused this function to run.
     *
     * @listens Player#seeked
     */
    handleSeeked(event: any): void;
    /**
     * Add the vjs-playing class to the element so it can change appearance.
     *
     * @param {EventTarget~Event} [event]
     *        The event that caused this function to run.
     *
     * @listens Player#play
     */
    handlePlay(event: any): void;
    /**
     * Add the vjs-paused class to the element so it can change appearance.
     *
     * @param {EventTarget~Event} [event]
     *        The event that caused this function to run.
     *
     * @listens Player#pause
     */
    handlePause(event: any): void;
    /**
     * Add the vjs-ended class to the element so it can change appearance
     *
     * @param {EventTarget~Event} [event]
     *        The event that caused this function to run.
     *
     * @listens Player#ended
     */
    handleEnded(event: any): void;
}
import Button from "../button.js";
//# sourceMappingURL=play-toggle.d.ts.map