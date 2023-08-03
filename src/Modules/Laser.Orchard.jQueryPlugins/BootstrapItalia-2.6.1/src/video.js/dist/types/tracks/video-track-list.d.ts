export default VideoTrackList;
/**
 * The current list of {@link VideoTrack} for a video.
 *
 * @see [Spec]{@link https://html.spec.whatwg.org/multipage/embedded-content.html#videotracklist}
 * @extends TrackList
 */
declare class VideoTrackList extends TrackList {
    changing_: boolean;
    removeTrack(rtrack: any): void;
}
import TrackList from "./track-list";
//# sourceMappingURL=video-track-list.d.ts.map