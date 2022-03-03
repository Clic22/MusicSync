﻿using App1.Models;
using System;
using Xunit;

namespace App1Tests.Models
{
    public class SongTest
    {
        [Fact]
        public void DefaultSongCreation()
        {
            Song song = new Song();
            Assert.NotNull(song);
            Assert.Null(song.Title);
            Assert.Null(song.LocalPath);
            Assert.Null(song.File);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
        }

        [Fact]
        public void DefaultSongCreationWithArguments()
        {
            string expected_title = "title";
            string expected_localPath = "path";
            string expected_file = "file";
            Song song = new Song(expected_title, expected_file, expected_localPath );

            Assert.NotNull(song);
            Assert.Equal(expected_title, song.Title);
            Assert.Equal(expected_localPath, song.LocalPath);
            Assert.Equal(expected_file, song.File);
            Assert.Equal(Song.SongStatus.upToDate, song.Status);
        }

        [Fact]
        public void SetStatusTriggerPropertyChanged()
        {
            Song song = new Song();
            Action action = () => song.Status = Song.SongStatus.locked;
            Assert.PropertyChanged(song, "Status", action);
        }
    }
}