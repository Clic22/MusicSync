using App1.Models;
using App1.Models.Ports;
using System.Collections.Generic;

namespace App1Tests.Mock
{
    public class SaverMock : ISaver
    {
        public SaverMock()
        {
            Songs = new List<Song>();
            User = new User();
        }

        public void saveUser(User user)
        {
            User = user;
        }

        public User savedUser()
        {
            return User;
        }

        public void saveSong(Song song)
        {
            Songs.Add(song);
        }

        public void unsaveSong(Song song)
        {
            Songs.Remove(song);
        }

        public List<Song> savedSongs()
        {
            return Songs;
        }

        List<Song> Songs;
        User User;

    }
}
