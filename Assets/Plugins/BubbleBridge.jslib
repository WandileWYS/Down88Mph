mergeInto(LibraryManager.library, {
  SendScoreToBubble: function (finalScore) {
    try {
      window.top.postMessage({ type: "GAME_OVER", score: finalScore }, "*");
    } catch (e) {
      console.warn("Could not send score to Bubble: ", e);
    }
  }
});