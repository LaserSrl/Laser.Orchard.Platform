export default AudioTrackList;
/**
 * The current list of {@link AudioTrack} for a media file.
 *
 * @see [Spec]{@link https://html.spec.whatwg.org/multipage/embedded-content.html#audiotracklist}
 * @extends TrackList
 */
declare class AudioTrackList extends TrackList {
    changing_: boolean;
    removeTrack(rtrack: any): void;
}
import TrackList from "./track-list";
//# sourceMappingURL=audio-track-list.d.ts.map