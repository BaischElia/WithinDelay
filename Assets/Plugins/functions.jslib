mergeInto(LibraryManager.library, {
    receiveGameData: function(data) {
        var gameData = UTF8ToString(data); 
        saveDataForDB(gameData);
    },
    receiveMidGameData: function(data) {
        var gameData = UTF8ToString(data); 
        saveHalfDataForDB(gameData);
    },
    gameEnd: function(isEnded) {
        console.log("end of experiment");
        prepareEndScreen();
    },
    receiveTrackingData: function(data) {
        var trackingData = UTF8ToString(data);
        saveTrackingData(trackingData);
    },
    receiveHardwareData: function(id, data) {
        saveHardwareData(UTF8ToString(id), UTF8ToString(data))
    }

});
