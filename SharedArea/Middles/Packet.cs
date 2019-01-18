
using System.Collections.Generic;
using SharedArea.Entities;

namespace SharedArea.Middles
{
    public class Packet
    {
        public string Status { get; set; }
        public string Email { get; set; }
        public string VerifyCode { get; set; }
        public Session Session { get; set; }
        public User User { get; set; }
        public UserSecret UserSecret { get; set; }
        public Contact Contact { get; set; }
        public ServiceMessage ServiceMessage { get; set; }
        public List<Contact> Contacts { get; set; }
        public Room Room { get; set; }
        public Complex Complex { get; set; }
        public ComplexSecret ComplexSecret { get; set; }
        public List<Workership> Workerships { get; set; }
        public Workership Workership { get; set; }
        public Bot Bot { get; set; }
        public List<Bot> Bots { get; set; }
        public List<Complex> Complexes { get; set; }
        public List<Room> Rooms { get; set; }
        public string SearchQuery { get; set; }
        public List<User> Users { get; set; }
        public List<Session> Sessions { get; set; }
        public List<Membership> Memberships { get; set; }
        public Membership Membership { get; set; }
        public List<File> Files { get; set; }
        public List<Message> Messages { get; set; }
        public File File { get; set; }
        public TextMessage TextMessage { get; set; }
        public PhotoMessage PhotoMessage { get; set; }
        public AudioMessage AudioMessage { get; set; }
        public VideoMessage VideoMessage { get; set; }
        public BotStoreHeader BotStoreHeader { get; set; }
        public List<BotStoreSection> BotStoreSections { get; set; }
        public BotSubscription BotSubscription { get; set; }
        public List<BotSubscription> BotSubscriptions { get; set; }
        public BotCreation BotCreation { get; set; }
        public List<BotCreation> BotCreations { get; set; }
        public Invite Invite { get; set; }
        public FileUsage FileUsage { get; set; }
        public BaseUser BaseUser { get; set; }
    }
}