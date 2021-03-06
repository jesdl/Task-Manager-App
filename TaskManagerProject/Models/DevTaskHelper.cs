﻿using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace TaskManagerProject.Models
{
    //[Authorize(Roles = "ProjectManager")]
    public class DevTaskHelper
    {
        public virtual Project Project { get; set; }
        public virtual DevTask DevTask { get; set; }

        static ApplicationDbContext db = new ApplicationDbContext();

        static UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>
            (new UserStore<ApplicationUser>(db));

        static RoleManager<IdentityRole> projectManager = new RoleManager<IdentityRole>
            (new RoleStore<IdentityRole>(db));

        [Authorize(Roles = "ProjectManager")]
        public List<string> GetAllTasks()
        {
            var result = db.DevTasks.Select(t => t.Name).ToList();

            return result;
        }

        public ICollection<int> TasksForUser(string userId)
        {
            var result = db.DevTasks.Where(up => up.ApplicationUsers.Select(d => d.Id).Contains(userId)).Select(t => t.Id);

            return result.ToList();
        }
        [Authorize(Roles = "Project Manager")]
        public ICollection<int> TasksForProject(int projectId)
        {
            var result = db.DevTasks.Where(up => up.Project.Id == projectId).Select(t => t.Id);

            return result.ToList();
        }

        [Authorize(Roles = "Project Manager")]
        public DevTask CreateDevTask( int id, string name, string description, DateTime deadline, int projectId)
        {
            DevTask newTask = new DevTask(id, name, description, deadline, projectId);

            return newTask;
        }

        [Authorize(Roles = "Project Manager")]
        public static void AssignDevTask(ApplicationUser user, DevTask task)
        {
            if(UserManager.checkUserRole(user.Id, "Developer"))
            {
                user.DevTasks.Add(task);
                task.ApplicationUsers.Add(user);
                db.SaveChanges();
            }
        }
        [Authorize(Roles = "Project Manager")]
        public static void AssignDevsToTask(List<ApplicationUser> devs, DevTask task)
        {
            foreach(ApplicationUser dev in devs)
            {
                if (!dev.DevTasks.Contains(task) && !task.ApplicationUsers.Contains(dev))
                {
                    AssignDevTask(dev, task);
                }
            }
        }
        [Authorize(Roles = "ProjectManager")]
        public static void UpdateDevTask(DevTask task)
        {
            db.SaveChanges();
        }
        [Authorize(Roles = "ProjectManager")]
        public static void  DeleteDevTask(DevTask task)
        {
            db.DevTasks.Remove(task);
            db.SaveChanges();
        }

        public static void AddComment(string comment, int id)
        {
            DevTask task = db.DevTasks.Find(id);
            if (task.Comments == null)
            {
                task.Comments = new List<string> { comment };
            }
            else
            {
                task.Comments.Add(comment);
            }
            
            db.SaveChanges();
        }
        //AddNote
        public static void UpdateCompletionPercent(double newValue, DevTask task)
        {
            if (newValue <= 100)
            {
                task.PercentCompleted = newValue;
                if (newValue == 100)
                {
                    task.IsComplete = true;
                    SendCompletionNotifications(task);
                }
                else
                {
                    task.IsComplete = false;
                }
                //db.SaveChanges();
            }
            else
            {
                //error
            }
        }
        public static void SendNotification(string title, string description, ApplicationUser user, DevTask task)
        {
            Notification notification = new Notification(title,  description, user.Id, task.Id, task.ProjectId);
            user.Notifications.Add(notification);
            task.Notification.Add(notification);
        }
        public static void SendNote(DevTask task, string title)
        {
            string fullTitle = title + " in " + task.Name;
            Note note = new Note(fullTitle, task.Id);
            task.Notes.Add(note);
        }
        //SendDeadlineAlert(Project) 
        public static void SendBugReport(DevTask task, string description)
        {
            string title = "Bug Report: " + task.Name; 
            int projectId = task.Project.Id;
            SendNote(task, description);
            List<ApplicationUser> recipients = db.Users.Where(u => userManager.IsInRole(u.Id, "ProjectManager")).ToList();
            foreach(ApplicationUser r in recipients)
            {
                SendNotification(title, description, r, task);
            }
        }
        public static void SendCompletionNotifications(DevTask task)
        {
            string title = "Task Completion Report: " + task.Name;
            int projectId = task.ProjectId;
            Project project = db.Projects.Find(projectId);

            string description = task.Name + " in " + project.Name + " has been completed.";
            List<ApplicationUser> recipients = db.Users.AsEnumerable().Where(u => UserManager.checkUserRole(u.Id, "Project Manager")).ToList();
            foreach (ApplicationUser r in recipients)
            {
                SendNotification(title, description, r, task);
            }
            
        }
        public static void SendDeadlineNotification(DevTask task)
        {
            string title = "Upcoming Deadline: " + task.Name;
            int projectId = task.ProjectId;
            Project project = db.Projects.Find(projectId);

            string description = task.Name + " in " + project.Name + " will be due in one day.";
            List<ApplicationUser> recipients = task.ApplicationUsers.Where(u => UserManager.checkUserRole(u.Id, "Developer")).ToList();
            foreach (ApplicationUser r in recipients)
            {
                SendNotification(title, description, r, task);
            }

        }
    }
}