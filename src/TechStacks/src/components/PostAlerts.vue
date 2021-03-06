<template>
    <div v-if="organization && post" class="post-alerts">
        <div v-if="post.labels && post.labels.length > 0" class="post-labels">

            <nuxt-link v-if="post.status" :class="`label ${post.status}`" :to="toUrl({ is: post.status })">
                {{ post.status }}
            </nuxt-link>

            <nuxt-link class="label" :style="labelStyle(label,organization)" :to="toUrl({ is: label })" v-for="label in post.labels" :key="label">
                {{ label }}
            </nuxt-link>

        </div>

        <v-alert v-if="post.archived" outline color="grey" icon="archive" :value="true">
            This is an archived post. You won't be able to vote or comment. Posts are automatically archived after 6 months.
        </v-alert>
        <v-alert v-else-if="post.locked != null" outline color="info" icon="lock" :value="true">
            This post is locked and contributions is limited to {{ organization.name }} moderators. {{ fromNow(post.locked) }}
        </v-alert>
        <v-alert v-else-if="organization.locked != null && !isOrganizationMember()" outline color="info" icon="lock" :value="true">
            Contributions to {{ organization.name }} is limited to members only
        </v-alert>
        <v-alert v-else-if="memberCannotComment()" outline color="error" icon="lock" :value="true">
            You are not permitted add comments
        </v-alert>
        <v-alert v-if="post.hidden != null" outline color="info" icon="remove_red_eye" :value="true">
            This post is hidden and will not be displayed in search results or news feeds. {{ fromNow(post.hidden) }}
        </v-alert>
    </div>  
</template>

<script>
import { routes } from "~/shared/routes";
import { fromNow } from "~/shared/utils";
import { isOrganizationModerator, isOrganizationMember, memberCannotComment, organizationMember, userId, labelStyle } from "~/shared/post";

export default {
  props: ["organization", "post"],
  methods: {
    toUrl(args) {
      const qs = { types: this.post.type };
      if (this.category)
        qs.c = this.category.slug;
      if (args)
        Object.assign(qs, args);

      return routes.organizationNews(this.organization.slug, qs);
    },

    isOrganizationModerator,
    isOrganizationMember,
    memberCannotComment,
    organizationMember, 
    getUserId:userId,
    labelStyle,
    fromNow,
  },

  data: () => ({
    routes
  })
};
</script>
