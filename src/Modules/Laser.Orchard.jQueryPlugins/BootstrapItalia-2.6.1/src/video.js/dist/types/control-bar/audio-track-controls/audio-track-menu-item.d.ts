export default AudioTrackMenuItem;
/**
 * An {@link AudioTrack} {@link MenuItem}
 *
 * @extends MenuItem
 */
declare class AudioTrackMenuItem extends MenuItem {
    track: any;
    createEl(type: any, props: any, attrs: any): Element;
    /**
     * Handle any {@link AudioTrack} change.
     *
     * @param {EventTarget~Event} [event]
     *        The {@link AudioTrackList#change} event that caused this to run.
     *
     * @listens AudioTrackList#change
     */
    handleTracksChange(event: any): void;
}
import MenuItem from "../../menu/menu-item.js";
//# sourceMappingURL=audio-track-menu-item.d.ts.map